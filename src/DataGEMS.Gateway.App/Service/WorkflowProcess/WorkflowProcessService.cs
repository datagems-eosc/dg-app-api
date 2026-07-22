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


		private async Task ExecuteOnboarding(DatasetPersist model, Guid processId, Guid stepId)
		{
			this._logger.Debug(new MapLogEntry("execute-onboarding").And("profilingModel", model).And("processId", processId).And("stepId", stepId));
			await this._authorizationService.AuthorizeForce(Permission.OnboardDataset);
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetOnboarding);
			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Common.WorkflowDefinitionKind.DatasetOnboarding) .ExcludeStaled(true) .CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetOnboarding.ToString(), nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", Common.WorkflowDefinitionKind.DatasetOnboarding.ToString(), nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();
			App.Model.WorkflowExecution execution = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = model.Id,
					workflow_process_id = processId,
					workflow_process_step_id = stepId,
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
				}
			}, new FieldSet(nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId)));
		}

		private async Task ExecuteCddIngestion(Guid datasetId, Guid processId, Guid stepId)
		{
			this._logger.Debug(new MapLogEntry("execute-cdd-ingestion").And("datasetId", datasetId).And("processId", processId).And("stepId", stepId));

			await this._authorizationService.AuthorizeForce(Permission.CddIngestDataset);
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetCddIngest);

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Common.WorkflowDefinitionKind.CDD_Ingest).ExcludeStaled(true).CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.CDD_Ingest.ToString(), nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", Common.WorkflowDefinitionKind.CDD_Ingest.ToString(), nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();

			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = datasetId,
					workflow_process_id = processId,
					workflow_process_step_id = stepId,
				}
			}, new FieldSet
			{
				Fields = [nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId),]
			});
		}

		private async Task ExecuteProfiling(ProfilingModel profilingModel, Guid processId, Guid stepId)
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
				.Kinds(Common.WorkflowDefinitionKind.DatasetProfiling)
				.ExcludeStaled(true)
				.CollectAsync();

			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetProfiling.ToString(), nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", Common.WorkflowDefinitionKind.DatasetProfiling.ToString(), nameof(App.Model.WorkflowDefinition)]);
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
				}
			}, new FieldSet
			{
				Fields = [
				nameof(App.Model.WorkflowExecution.Id),
				nameof(App.Model.WorkflowExecution.WorkflowId),
				]
			});
		}

		private async Task ExecutePackaging(Guid datasetId, Guid processId, Guid stepId)
		{
			this._logger.Debug(new MapLogEntry("execute-packaging").And("datasetId", datasetId).And("processId", processId).And("stepId", stepId));

			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetPackaging);
			await this._authorizationService.AuthorizeForce(Permission.PackageDataset);

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Common.WorkflowDefinitionKind.DatasetPackaging).ExcludeStaled(true).CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetPackaging.ToString(), nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", Common.WorkflowDefinitionKind.DatasetPackaging.ToString(), nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();

			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = datasetId,
					workflow_process_id = processId,
					workflow_process_step_id = stepId,
				}
			}, new FieldSet
			{
				Fields = [nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId),]
			});
		}

		private async Task ExecuteRecommendationRegistering(Guid datasetId, Guid processId, Guid stepId)
		{
			await this._authorizationService.AuthorizeForce(Permission.RecommendationRegisterDataset);
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetRecommendationRegistering);

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Kinds(Common.WorkflowDefinitionKind.DatasetRecommendationRegistering).ExcludeStaled(true).CollectAsync();
			if (definitions == null || definitions.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetRecommendationRegistering.ToString(), nameof(App.Model.WorkflowDefinition)]);
			if (definitions.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", Common.WorkflowDefinitionKind.DatasetRecommendationRegistering.ToString(), nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();
			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = datasetId,
					workflow_process_id = processId,
					workflow_process_step_id = stepId,
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
