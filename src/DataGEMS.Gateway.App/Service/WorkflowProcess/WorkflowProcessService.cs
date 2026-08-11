using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.AAI;
using DataGEMS.Gateway.App.Service.Airflow;
using DataGEMS.Gateway.App.Service.Storage;
using DataGEMS.Gateway.App.Service.WorkflowProcess.Model;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Service.WorkflowProcess
{
	public class WorkflowProcessService : IWorkflowProcessService
	{
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IStorageService _storageService;
		private readonly ILogger<WorkflowProcessService> _logger;
		private readonly WorkflowProcessConfig _config;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;
		private readonly IAAIService _aaiService;
		private readonly IAirflowService _airflowService;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Data.AppDbContext _dbContext;

		public WorkflowProcessService(
			ILogger<WorkflowProcessService> logger,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			QueryFactory queryFactory,
			IAAIService aaiService,
			WorkflowProcessConfig config,
			IStorageService storageService,
			IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
			IStringLocalizer<Resources.MySharedResources> localizer,
			ErrorThesaurus errors,
			EventBroker eventBroker,
			IAirflowService airflowService,
			JsonHandlingService jsonHandlingService,
			Data.AppDbContext dbContext)
		{
			this._logger = logger;
			this._builderFactory = builderFactory;
			this._deleterFactory = deleterFactory;
			this._queryFactory = queryFactory;
			this._aaiService = aaiService;
			this._storageService = storageService;
			this._config = config;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._localizer = localizer;
			this._errors = errors;
			this._eventBroker = eventBroker;
			this._airflowService = airflowService;
			this._jsonHandlingService = jsonHandlingService;
			this._dbContext = dbContext;
		}

		public async Task UpdateWorkflowProcessStep(WorkflowProcessStepPersist model)
		{
			this._logger.Debug(new MapLogEntry("update-workflow-process-step").And("model", model));
			await this._authorizationService.AuthorizeForce(Permission.EditWorkflowProcessStep);

			Data.WorkflowProcessStep data = await this._queryFactory.Query<WorkflowProcessStepQuery>().Ids(model.Id.Value).FirstAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(App.Model.WorkflowProcessStep)]);

			data.UpdatedAt = DateTime.UtcNow;
			data.Status = model.Status.Value;
			data.WorkflowTaskInstanceDetails += model.WorkflowTaskInstanceDetails + "\n";
			this._dbContext.Update(data);
			await this._dbContext.SaveChangesAsync();
			this._eventBroker.EmitWorkflowProcessStepTouched(data.Id);

			if (model.Status.Value == Common.Enum.WorkflowProcessStatus.Failed)
			{
				Data.WorkflowProcess workflowProcess = await this._queryFactory.Query<WorkflowProcessQuery>().Ids(data.ProcessId).FirstAsync();
				workflowProcess.Status = Common.Enum.WorkflowProcessStatus.Failed;
				workflowProcess.UpdatedAt = DateTime.UtcNow;
				this._dbContext.Update(workflowProcess);
				await this._dbContext.SaveChangesAsync();
				this._eventBroker.EmitWorkflowProcessTouched(workflowProcess.Id);
			}
		}

		public async Task FinilizeOnboardingStep(WorkflowProcessStepPersist model, DatasetProfiling profiling)
		{
			await this.UpdateWorkflowProcessStep(model);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetOnboarding);
			List<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps = configuration.Steps.OrderBy(x => x.Order).ToList();

			Data.WorkflowProcessStep data = await this._queryFactory.Query<WorkflowProcessStepQuery>().ProcessIds(model.ProcessId.Value).StepIds(steps[1].Id).FirstAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(App.Model.WorkflowProcessStep)]);

			await this.ExecuteProfiling(new ProfilingModel
			{
				DatabaseName = profiling.DatabaseName,
				DatasetId = profiling.Id.Value,
				Kind = profiling.DataStoreKind.Value
			}, data.ProcessId, data.Id, data.StepId, steps[1].TaskId);
		}

		public async Task FinilizeProfilingStep(WorkflowProcessStepPersist model, Guid datasetId)
		{
			await this.UpdateWorkflowProcessStep(model);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetOnboarding);
			List<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps = configuration.Steps.OrderBy(x => x.Order).ToList();

			Data.WorkflowProcessStep data = await this._queryFactory.Query<WorkflowProcessStepQuery>().ProcessIds(model.ProcessId.Value).StepIds(steps[2].Id).FirstAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(App.Model.WorkflowProcessStep)]);

			await this.ExecutePackaging(datasetId, data.ProcessId, data.Id, data.StepId, steps[2].TaskId);
		}

		public async Task FinilizePackagingStep(WorkflowProcessStepPersist model, Guid datasetId)
		{
			await this.UpdateWorkflowProcessStep(model);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetOnboarding);
			List<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps = configuration.Steps.OrderBy(x => x.Order).ToList();

			Data.WorkflowProcessStep data = await this._queryFactory.Query<WorkflowProcessStepQuery>().ProcessIds(model.ProcessId.Value).StepIds(steps[3].Id).FirstAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(App.Model.WorkflowProcessStep)]);

			await this.ExecuteRecommendationRegistering(datasetId, data.ProcessId, data.Id, data.StepId, steps[3].TaskId);
		}

		public async Task FinilizeRecommendationStep(WorkflowProcessStepPersist model, Guid datasetId)
		{
			await this.UpdateWorkflowProcessStep(model);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetOnboarding);
			List<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps = configuration.Steps.OrderBy(x => x.Order).ToList();

			Data.WorkflowProcessStep data = await this._queryFactory.Query<WorkflowProcessStepQuery>().ProcessIds(model.ProcessId.Value).StepIds(steps[4].Id).FirstAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(App.Model.WorkflowProcessStep)]);

			await this.ExecuteCddIngestion(datasetId, data.ProcessId, data.Id, data.StepId, steps[4].TaskId);
		}

		public async Task FinilizeCddIngestionStep(WorkflowProcessStepPersist model, Guid datasetId)
		{
			await this.UpdateWorkflowProcessStep(model);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetOnboarding);
			Data.WorkflowProcess process = await this._queryFactory.Query<WorkflowProcessQuery>().Ids(model.ProcessId.Value).FirstAsync();
			if (process == null) throw new DGNotFoundException(this._localizer["general_notFound", model.ProcessId.Value, nameof(App.Model.WorkflowProcess)]);
			process.Status = Common.Enum.WorkflowProcessStatus.Succeeded;
			process.UpdatedAt = DateTime.UtcNow;
			this._dbContext.Update(process);
			await this._dbContext.SaveChangesAsync();
			this._eventBroker.EmitWorkflowProcessTouched(process.Id);
		}

		public async Task<App.Model.WorkflowProcess> ExecuteOnboardingFlow(DatasetPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("execute-onboarding-flow").And("profilingModel", model).And("fields", fields));

			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetOnboarding);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetOnboarding);
			Guid datasetId = Guid.NewGuid();
			(Data.WorkflowProcess data, IOrderedEnumerable<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps, List<Data.WorkflowProcessStep> stepData) = await this.PersistFlow(configuration, datasetId);

			try
			{
				await this.ExecuteOnboarding(
					model: model, 
					id: stepData.First().Id, 
					processId: data.Id, 
					stepId: stepData.First().StepId, 
					identifyingTag: steps.First().TaskId, 
					datasetId: datasetId);
			}
			catch
			{
				await PersistFailedFlow(data, stepData);
				throw;
			}

			App.Model.WorkflowProcess persisted = await this._builderFactory.Builder<App.Model.Builder.WorkflowProcessBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.WorkflowProcess.Id)), data);
			return persisted;
		}

		public async Task<App.Model.WorkflowProcess> ExecuteProfilingFlow(DatasetProfiling model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("execute-profiling-flow").And("profilingModel", model).And("fields", fields));

			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetProfiling);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetProfiling);
			(Data.WorkflowProcess data, IOrderedEnumerable<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps, List<Data.WorkflowProcessStep> stepData) = await this.PersistFlow(configuration, model.Id.Value);

			try
			{
				await this.ExecuteProfiling(new ProfilingModel
				{
					DatabaseName = model.DatabaseName,
					DatasetId = model.Id.Value,
					Kind = model.DataStoreKind.Value,
				}, stepData.First().Id, data.Id, stepData.First().StepId, steps.First().TaskId);
			}
			catch
			{
				await PersistFailedFlow(data, stepData);
				throw;
			}

			App.Model.WorkflowProcess persisted = await this._builderFactory.Builder<App.Model.Builder.WorkflowProcessBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.WorkflowProcess.Id)), data);
			return persisted;
		}


		public async Task<App.Model.WorkflowProcess> ExecutePackagingFlow(App.Model.DatasetPackaging model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("execute-packaging-flow").And("DatasetPackaging", model).And("fields", fields));

			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetPackaging);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetPackaging);
			(Data.WorkflowProcess data, IOrderedEnumerable<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps, List<Data.WorkflowProcessStep> stepData) = await this.PersistFlow(configuration, model.Id.Value);

			try
			{
				await this.ExecutePackaging(model.Id.Value, stepData.First().Id, data.Id, stepData.First().StepId, steps.First().TaskId);
			}
			catch
			{
				await PersistFailedFlow(data, stepData);
				throw;
			}

			App.Model.WorkflowProcess persisted = await this._builderFactory.Builder<App.Model.Builder.WorkflowProcessBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.WorkflowProcess.Id)), data);
			return persisted;
		}

		public async Task<App.Model.WorkflowProcess> ExecuteRecommendationFlow(App.Model.DatasetRecommendationRegistering model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("execute-recommendation-registering-flow").And("DatasetRecommendationRegistering", model).And("fields", fields));

			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetRecommendationRegistering);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.DatasetRecommendationRegistering);
			(Data.WorkflowProcess data, IOrderedEnumerable<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps, List<Data.WorkflowProcessStep> stepData) = await this.PersistFlow(configuration, model.Id.Value);

			try
			{
				await this.ExecutePackaging(model.Id.Value, stepData.First().Id, data.Id, stepData.First().StepId, steps.First().TaskId);
			}
			catch
			{
				await PersistFailedFlow(data, stepData);
				throw;
			}

			App.Model.WorkflowProcess persisted = await this._builderFactory.Builder<App.Model.Builder.WorkflowProcessBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.WorkflowProcess.Id)), data);
			return persisted;
		}

		public async Task<App.Model.WorkflowProcess> ExecuteCddIngestionFlow(App.Model.DatasetCddIngest model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("execute-cdd-ingestion-flow").And("DatasetCddIngest", model).And("fields", fields));

			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetCddIngest);

			WorkflowProcessConfig.WorkflowProcessConfigItem configuration = this._config.Items.FirstOrDefault(x => x.Kind == Common.WorkflowProcessKind.CDD_Ingest);
			(Data.WorkflowProcess data, IOrderedEnumerable<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps, List<Data.WorkflowProcessStep> stepData) = await this.PersistFlow(configuration, model.Id.Value);

			try
			{
				await this.ExecutePackaging(model.Id.Value, stepData.First().Id, data.Id, stepData.First().StepId, steps.First().TaskId);
			}
			catch
			{
				await PersistFailedFlow(data, stepData);
				throw;
			}

			App.Model.WorkflowProcess persisted = await this._builderFactory.Builder<App.Model.Builder.WorkflowProcessBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.WorkflowProcess.Id)), data);
			return persisted;
		}

		private async Task PersistFailedFlow(Data.WorkflowProcess data, List<Data.WorkflowProcessStep> stepData)
		{
			DateTime now = DateTime.UtcNow;
			foreach (var item in stepData)
			{
				item.Status = Common.Enum.WorkflowProcessStatus.Failed;
				item.UpdatedAt = now;
			}
			this._dbContext.UpdateRange(stepData);
			await this._dbContext.SaveChangesAsync();
			this._eventBroker.EmitWorkflowProcessStepTouched(stepData.Select(x => x.Id));

			data.Status = Common.Enum.WorkflowProcessStatus.Failed;
			data.UpdatedAt = now;
			this._dbContext.Update(data);
			await this._dbContext.SaveChangesAsync();
			this._eventBroker.EmitWorkflowProcessTouched(data.Id);
		}

		private async Task<(Data.WorkflowProcess data, IOrderedEnumerable<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps, List<Data.WorkflowProcessStep> stepData)> PersistFlow(WorkflowProcessConfig.WorkflowProcessConfigItem configuration, Guid? datasetId = null)
		{
			DateTime now = DateTime.UtcNow;

			Data.WorkflowProcess data = new Data.WorkflowProcess
			{
				Id = Guid.NewGuid(),
				ProcessId = configuration.Id,
				Status = Common.Enum.WorkflowProcessStatus.InProgress,
				UserId = await this._authorizationContentResolver.CurrentUserId(),
				DatasetId = datasetId,
				CreatedAt = now,
				UpdatedAt = now,
			};
			this._dbContext.Add(data);
			await this._dbContext.SaveChangesAsync();
			this._eventBroker.EmitWorkflowProcessTouched(data.Id);

			IOrderedEnumerable<WorkflowProcessConfig.WorkflowProcessConfigItem.WorkflowProcessConfigItemStep> steps = configuration.Steps.OrderBy(x => x.Order);
			List<Data.WorkflowProcessStep> stepData = steps.Select(x => new Data.WorkflowProcessStep
			{
				Id = Guid.NewGuid(),
				StepId = x.Id,
				ProcessId = data.Id,
				Status = Common.Enum.WorkflowProcessStatus.InProgress,
				WorkflowTaskInstanceDetails = "",
				CreatedAt = now,
				UpdatedAt = now,
			}).ToList();
			this._dbContext.AddRange(stepData);
			await this._dbContext.SaveChangesAsync();
			this._eventBroker.EmitWorkflowProcessStepTouched(stepData.Select(x => x.Id));
			return (data, steps, stepData);
		}

		private async Task ExecuteOnboarding(DatasetPersist model, Guid id, Guid processId, Guid stepId, string identifyingTag, Guid? datasetId = null)
		{
			this._logger.Debug(new MapLogEntry("execute-onboarding").And("model", model).And("processId", processId).And("stepId", stepId));
			await this._authorizationService.AuthorizeForce(Permission.OnboardDataset);
			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Enum.Parse<Common.WorkflowDefinitionKind>(identifyingTag)).ExcludeStaled(true).CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();
			App.Model.WorkflowExecution execution = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = datasetId ?? Guid.NewGuid(),
					name = model.Name,
					description = model.Description,
					headline = model.Headline,
					fields_of_science = model.FieldOfScience,
					languages = model.Language,
					keywords = model.Keywords,
					countries = model.Country,
					publishedUrl = model.Url,
					citeAs = model.CiteAs,
					license = model.License,
					dataLocations = this._jsonHandlingService.ToJsonSafe(model.DataLocations.Select(x => new
					{
						kind = x.Kind,
						location = x.Location,
					})),
					date_published = model.DatePublished,
					userId = await this._authorizationContentResolver.CurrentUserId(),
					doi = model.Doi,
					workflow_process_step_information = new
					{
						id = id,
						process_id = processId,
						step_id = stepId
					}
				}
			}, new FieldSet(nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId)));
		}

		private async Task ExecuteCddIngestion(Guid datasetId, Guid processId, Guid stepId, Guid stepIdentifier, string identifyingTag)
		{
			this._logger.Debug(new MapLogEntry("execute-cdd-ingestion").And("datasetId", datasetId).And("processId", processId).And("stepId", stepId));

			await this._authorizationService.AuthorizeForce(Permission.CddIngestDataset);
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetCddIngest);

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Enum.Parse<Common.WorkflowDefinitionKind>(identifyingTag)).ExcludeStaled(true).CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();

			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = datasetId,
					workflow_process_step_information = new
					{
						id = stepId,
						step_id = stepIdentifier,
						process_id = processId
					},
				}
			}, new FieldSet
			{
				Fields = [nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId),]
			});
		}

		private async Task ExecuteProfiling(ProfilingModel profilingModel, Guid processId, Guid stepId, Guid stepIdentifier, string identifyingTag)
		{
			this._logger.Debug(new MapLogEntry("execute-profiling").And("profilingModel", profilingModel).And("processId", processId).And("stepId", stepId));
			await this._authorizationService.AuthorizeForce(Permission.ProfileDataset);
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetProfiling);

			List<DataManagement.Model.Dataset> datas = (await this._queryFactory.Query<DatasetHttpQuery>().Ids(profilingModel.DatasetId).CollectAsync())?.Items ?? [];
			if (datas == null || datas.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", profilingModel.DatasetId, nameof(App.Model.Dataset)]);
			if (datas.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", profilingModel.DatasetId, nameof(App.Model.Dataset)]);
			App.Model.Dataset model = await this._builderFactory.Builder<App.Model.Builder.DatasetBuilder>().Build(new FieldSet(
				nameof(App.Model.Dataset.Id),
				nameof(App.Model.Dataset.Name),
				nameof(App.Model.Dataset.Description),
				nameof(App.Model.Dataset.Headline),
				nameof(App.Model.Dataset.FieldOfScience),
				nameof(App.Model.Dataset.Language),
				nameof(App.Model.Dataset.Keywords),
				nameof(App.Model.Dataset.Country),
				nameof(App.Model.Dataset.Url),
				nameof(App.Model.Dataset.Doi),
				nameof(App.Model.Dataset.DatePublished),
				nameof(App.Model.Dataset.CiteAs),
				nameof(App.Model.Dataset.License),
				nameof(App.Model.Dataset.ArchivedAt),
				nameof(App.Model.Dataset.Status)), datas.First());

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>()
				.Kinds(Common.WorkflowDefinitionKind.DatasetProfiling_test)
				.ExcludeStaled(true)
				.CollectAsync();

			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();
			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = model.Id,
					name = model.Name,
					description = model.Description,
					headline = model.Headline,
					fields_of_science = model.FieldOfScience,
					languages = model.Language,
					keywords = model.Keywords,
					countries = model.Country,
					url = model.Url,
					doi = model.Doi,
					date_published = model.DatePublished,
					citeAs = model.CiteAs,
					license = model.License,
					dataset_file_path = await this._storageService.DirectoryOf(Common.StorageType.Dataset, model.Id.ToString()),
					userId = await this._authorizationContentResolver.CurrentUserId(),
					data_store_kind = profilingModel.Kind,
					archivedAt = model.ArchivedAt,
					database_name = profilingModel.DatabaseName,
					workflow_process_step_information = new
					{
						id = stepId,
						step_id = stepIdentifier,
						process_id = processId,
					},
				}
			}, new FieldSet
			{
				Fields = [
				nameof(App.Model.WorkflowExecution.Id),
				nameof(App.Model.WorkflowExecution.WorkflowId),
				]
			});
		}

		private async Task ExecutePackaging(Guid datasetId, Guid processId, Guid stepId, Guid stepIdentifier, string identifyingTag)
		{
			this._logger.Debug(new MapLogEntry("execute-packaging").And("datasetId", datasetId).And("processId", processId).And("stepId", stepId));

			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetPackaging);
			await this._authorizationService.AuthorizeForce(Permission.PackageDataset);

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Enum.Parse<Common.WorkflowDefinitionKind>(identifyingTag)).ExcludeStaled(true).CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();

			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = datasetId,
					workflow_process_step_information = new
					{
						id = stepId,
						step_id = stepIdentifier,
						process_id = processId,
					},
				},
			}, new FieldSet
			{
				Fields = [nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId),]
			});
		}

		private async Task ExecuteRecommendationRegistering(Guid datasetId, Guid processId, Guid stepId, Guid stepIdentifier, string identifyingTag)
		{
			await this._authorizationService.AuthorizeForce(Permission.RecommendationRegisterDataset);
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetRecommendationRegistering);

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Enum.Parse<Common.WorkflowDefinitionKind>(identifyingTag)).ExcludeStaled(true).CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", identifyingTag, nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();
			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = datasetId,
					workflow_process_step_information = new
					{
						id = stepId,
						step_id = stepIdentifier,
						process_id = processId,
					},
				}
			}, new FieldSet
			{
				Fields = [
				nameof(App.Model.WorkflowExecution.Id),
				nameof(App.Model.WorkflowExecution.WorkflowId),
				]
			});
		}
	}
}
