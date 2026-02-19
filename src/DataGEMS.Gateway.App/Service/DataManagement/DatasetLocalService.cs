using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.AAI;
using DataGEMS.Gateway.App.Service.Airflow;
using DataGEMS.Gateway.App.Service.DataManagement.Model;
using DataGEMS.Gateway.App.Service.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
	public class DatasetLocalService : IDatasetService
	{
		private readonly Data.DataManagementDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IStorageService _storageService;
		private readonly ILogger<DatasetLocalService> _logger;
		private readonly AAIConfig _aaiConfig;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;
		private readonly IAAIService _aaiService;
		private readonly IAirflowService _airflowService;
		private readonly JsonHandlingService _jsonHandlingService;

		public DatasetLocalService(
			ILogger<DatasetLocalService> logger,
			Data.DataManagementDbContext dbContext,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			QueryFactory queryFactory,
			IAAIService aaiService,
			AAIConfig aaiConfig,
			IStorageService storageService,
			IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
			IStringLocalizer<Resources.MySharedResources> localizer,
			ErrorThesaurus errors,
			EventBroker eventBroker,
			IAirflowService airflowService,
			JsonHandlingService jsonHandlingService)
		{
			this._logger = logger;
			this._dbContext = dbContext;
			this._builderFactory = builderFactory;
			this._deleterFactory = deleterFactory;
			this._queryFactory = queryFactory;
			this._aaiService = aaiService;
			this._storageService = storageService;
			this._aaiConfig = aaiConfig;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._localizer = localizer;
			this._errors = errors;
			this._eventBroker = eventBroker;
			this._airflowService = airflowService;
			this._jsonHandlingService = jsonHandlingService;
		}

		private async Task AuthorizeExecuteOnboardingWorkflowForce()
		{
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetOnboarding);
		}

		private async Task AuthorizeExecuteProfilingWorkflowForce()
		{
			await this._authorizationService.AuthorizeForce(Permission.CanExecuteDatasetProfiling);
		}

		private async Task AuthorizeCreateForce()
		{
			await this._authorizationService.AuthorizeForce(Permission.OnboardDataset);
		}

		private async Task AuthorizeProfileForce()
		{
			await this._authorizationService.AuthorizeForce(Permission.ProfileDataset);
		}

		private async Task AutoAssignNewDatasetRoles(Guid datasetId)
		{
			if (this._aaiConfig.AutoAssignGrantsOnNewDataset == null || this._aaiConfig.AutoAssignGrantsOnNewDataset.Count == 0) return;

			String subjectId = await this._authorizationContentResolver.SubjectIdOfCurrentUser();
			await this._aaiService.BootstrapUserContextGrants(subjectId);
			await this._aaiService.AssignDatasetGrantToUser(subjectId, datasetId, this._aaiConfig.AutoAssignGrantsOnNewDataset);
		}

		public async Task<Guid> FutureOnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("future onboarding").And("type", nameof(App.Model.DatasetPersist)).And("model", model).And("fields", fields));

			await this.AuthorizeCreateForce();
			await this.AuthorizeExecuteOnboardingWorkflowForce();

			model.Id = Guid.NewGuid();

			foreach (Common.DataLocation location in model.DataLocations.Where(x => x.Kind == Common.DataLocationKind.File))
			{
				String stagedPath = await this._storageService.MoveToStorage(location.Location, Common.StorageType.DatasetOnboardStaging, model.Id.ToString());
				location.Location = stagedPath;
			}

			await this.ExecuteFutureOnboardingFlow(model);

			await this.AutoAssignNewDatasetRoles(model.Id.Value);
			this._eventBroker.EmitDatasetTouched(model.Id.Value);

			return model.Id.Value;
		}

		private async Task ExecuteFutureOnboardingFlow(App.Model.DatasetPersist model)
		{
			this._logger.Debug(new MapLogEntry("executing").And("type", nameof(ExecuteFutureOnboardingFlow)).And("model", model));

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>()
				.Kinds(Common.WorkflowDefinitionKind.DatasetOnboardingFuture)
				.ExcludeStaled(true)
				.CollectAsync();

			if (definitions == null || definitions.Count != 1) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetOnboardingFuture.ToString(), nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();

			App.Model.WorkflowExecution execution = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
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
					publishedUrl = model.Url,
					size = model.Size,
					citeAs = model.CiteAs,
					conformsTo = model.ConformsTo,
					license = model.License,
					dataLocations = this._jsonHandlingService.ToJsonSafe(model.DataLocations.Select(x => new
					{
						kind = x.Kind,
						location = x.Location,
					})),
					version = model.Version,
					mime_type = model.MimeType,
					date_published = model.DatePublished,
					code = model.Code,
					userId = await this._authorizationContentResolver.CurrentUserId(),
				}
			}, new FieldSet(nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId)));
		}

		public async Task<Guid> FutureProfileAsync(App.Model.DatasetProfiling viewModel)
		{
			this._logger.Debug(new MapLogEntry("future profiling").And("model", viewModel));

			await this.AuthorizeProfileForce();
			await this.AuthorizeExecuteProfilingWorkflowForce();

			List<Dataset> datas = await this._queryFactory.Query<DatasetHttpQuery>()
				.Ids(viewModel.Id.Value)
				.State(Common.Enum.DatasetState.Loaded)
				.CollectAsync();
			if (datas == null || datas.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", viewModel.Id.Value, nameof(App.Model.Dataset)]);
			if (datas.Count > 1) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetProfilingFuture.ToString(), nameof(App.Model.Dataset)]);

			FieldSet fields = new FieldSet(
				nameof(App.Model.Dataset.Id),
				nameof(App.Model.Dataset.Code),
				nameof(App.Model.Dataset.Name),
				nameof(App.Model.Dataset.Description),
				nameof(App.Model.Dataset.License),
				nameof(App.Model.Dataset.MimeType),
				nameof(App.Model.Dataset.Size),
				nameof(App.Model.Dataset.Url),
				nameof(App.Model.Dataset.Version),
				nameof(App.Model.Dataset.Headline),
				nameof(App.Model.Dataset.Keywords),
				nameof(App.Model.Dataset.FieldOfScience),
				nameof(App.Model.Dataset.Language),
				nameof(App.Model.Dataset.Country),
				nameof(App.Model.Dataset.DatePublished),
				nameof(App.Model.Dataset.ArchivedAt),
				nameof(App.Model.Dataset.ConformsTo),
				nameof(App.Model.Dataset.CiteAs),
				nameof(App.Model.Dataset.Status));
			App.Model.Dataset model = await this._builderFactory.Builder<App.Model.Builder.DatasetBuilder>().Build(fields, datas.First());
			await this.ExecuteFutureProfilingFlow(model, viewModel.DataStoreKind);

			return viewModel.Id.Value;
		}

		private async Task ExecuteFutureProfilingFlow(App.Model.Dataset model, DataStoreKind? dataStoreKind)
		{
			this._logger.Debug(new MapLogEntry("executing").And("type", nameof(ExecuteFutureProfilingFlow)).And("model", model));

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>()
				.Kinds(Common.WorkflowDefinitionKind.DatasetProfilingFuture)
				.ExcludeStaled(true)
				.CollectAsync();

			if (definitions == null || definitions.Count != 1) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetProfilingFuture.ToString(), nameof(App.Model.WorkflowDefinition)]);
			Airflow.Model.AirflowDag selectedDefinition = definitions.FirstOrDefault();
			_ = await this._airflowService.ExecuteWorkflowAsync(new App.Model.WorkflowExecutionArgs
			{
				WorkflowId = selectedDefinition.Id,
				Configurations = new
				{
					id = model.Id,
					code = model.Code,
					name = model.Name,
					description = model.Description,
					license = model.License,
					mime_type = model.MimeType,
					size = model.Size,
					url = model.Url,
					version = model.Version,
					headline = model.Headline,
					keywords = model.Keywords,
					fields_of_science = model.FieldOfScience,
					languages = model.Language,
					countries = model.Country,
					date_published = model.DatePublished,
					dataset_file_path = await this._storageService.DirectoryOf(Common.StorageType.Dataset, model.Id.ToString()),
					userId = await this._authorizationContentResolver.CurrentUserId(),
					data_store_kind = dataStoreKind,
					citeAs = model.CiteAs,
					conformsTo = model.ConformsTo,
					archivedAt = model.ArchivedAt,
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
