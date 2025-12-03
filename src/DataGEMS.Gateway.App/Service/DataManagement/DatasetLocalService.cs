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

		private async Task AuthorizeEditForce(Guid datasetId)
		{
			await this.AuthorizeForce(datasetId, Permission.EditDataset);
		}

		private async Task AuthorizDeleteForce(Guid datasetId)
		{
			await this.AuthorizeForce(datasetId, Permission.DeleteDataset);
		}

		private async Task AuthorizeForce(Guid datasetId, String permission)
		{
			HashSet<string> userDatasetGroupRoles = await _authorizationContentResolver.EffectiveContextRolesForDatasetOfUser(datasetId);
			await this._authorizationService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(userDatasetGroupRoles), permission);
		}

		private async Task AutoAssignNewDatasetRoles(Guid datasetId)
		{
			if (this._aaiConfig.AutoAssignGrantsOnNewDataset == null || this._aaiConfig.AutoAssignGrantsOnNewDataset.Count == 0) return;

			String subjectId = await this._authorizationContentResolver.SubjectIdOfCurrentUser();
			await this._aaiService.BootstrapUserContextGrants(subjectId);
			await this._aaiService.AssignDatasetGrantToUser(subjectId, datasetId, this._aaiConfig.AutoAssignGrantsOnNewDataset);
		}

		public async Task<Guid> OnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("onboarding").And("type", nameof(App.Model.DatasetPersist)).And("model", model).And("fields", fields));

			await this.AuthorizeCreateForce();
			await this.AuthorizeExecuteOnboardingWorkflowForce();

			model.Id = Guid.NewGuid();
			if (model.DataLocations != null && model.DataLocations.Any(x => x.Kind == Common.DataLocationKind.Staged))
			{
				string relativePath = this._storageService.GetRelativePath(model.DataLocations.FirstOrDefault().Url, Common.StorageType.DatasetOnboardStaging);
				model.Id = Guid.Parse(relativePath.Split('/')[0]);
			}

			foreach (Common.DataLocation location in model.DataLocations.Where(x => x.Kind == Common.DataLocationKind.File))
			{
				String stagedPath = await this._storageService.MoveToStorage(location.Url, Common.StorageType.DatasetOnboardStaging, model.Id.ToString());
				location.Url = stagedPath;
			}

			await this.ExecuteOnboardingFlow(model);

			await this.AutoAssignNewDatasetRoles(model.Id.Value);
			this._eventBroker.EmitDatasetTouched(model.Id.Value);

			return model.Id.Value;
		}

		public async Task<Guid> FutureOnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("future onboarding").And("type", nameof(App.Model.DatasetPersist)).And("model", model).And("fields", fields));

			await this.AuthorizeCreateForce();
			await this.AuthorizeExecuteOnboardingWorkflowForce();

			model.Id = Guid.NewGuid();

			foreach (Common.DataLocation location in model.DataLocations.Where(x => x.Kind == Common.DataLocationKind.File))
			{
				String stagedPath = await this._storageService.MoveToStorage(location.Url, Common.StorageType.DatasetOnboardStaging, model.Id.ToString());
				location.Url = stagedPath;
			}

			await this.ExecuteFutureOnboardingFlow(model);

			await this.AutoAssignNewDatasetRoles(model.Id.Value);
			this._eventBroker.EmitDatasetTouched(model.Id.Value);

			return model.Id.Value;
		}

		private async Task ExecuteOnboardingFlow(App.Model.DatasetPersist model)
		{
			this._logger.Debug(new MapLogEntry("executing").And("type", nameof(ExecuteOnboardingFlow)).And("model", model));

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>()
				.Kinds(Common.WorkflowDefinitionKind.DatasetOnboarding)
				.ExcludeStaled(true)
				.CollectAsync();

			if (definitions == null || definitions.Count != 1) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetOnboarding.ToString(), nameof(App.Model.WorkflowDefinition)]);
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
						url = x.Url,
					})),
					version = model.Version,
					mime_type = model.MimeType,
					date_published = model.DatePublished,
					code = model.Code,
					userId = await this._authorizationContentResolver.CurrentUserId(),
				}
			}, new FieldSet(nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId)));
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
						url = x.Url,
					})),
					version = model.Version,
					mime_type = model.MimeType,
					date_published = model.DatePublished,
					code = model.Code,
					userId = await this._authorizationContentResolver.CurrentUserId(),
				}
			}, new FieldSet(nameof(App.Model.WorkflowExecution.Id), nameof(App.Model.WorkflowExecution.WorkflowId)));
		}

		public async Task<Guid> OnboardAsDataManagementAsync(App.Model.DatasetPersist model)
		{
			this._logger.Debug(new MapLogEntry("onboarding as data management").And("type", nameof(App.Model.DatasetPersist)).And("model", model));

			await this.AuthorizeCreateForce();

			Service.DataManagement.Model.Dataset data = await this.PatchAndSave(model, false);

			if (model.DataLocations != null && model.DataLocations.Any(x => x.Kind == Common.DataLocationKind.Staged))
			{
				await this._storageService.CreateDirectoryPath(Common.StorageType.Dataset, model.DataLocations.FirstOrDefault().Url);
			}
			else
			{
				String datasetPath = await this._storageService.DirectoryOf(Common.StorageType.DatasetOnboardStaging, data.Id.ToString());
				await this._storageService.MoveToStorage(datasetPath, Common.StorageType.Dataset);
			}

			return data.Id;
		}

		public async Task<Guid> ProfileAsync(Guid id)
		{
			this._logger.Debug(new MapLogEntry("profiling").And("id", id));

			await this.AuthorizeProfileForce();
			await this.AuthorizeExecuteProfilingWorkflowForce();

			Data.Dataset data = await this._dbContext.Datasets.FindAsync(id);
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);
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
				nameof(App.Model.Dataset.DatePublished));
			App.Model.Dataset model = await this._builderFactory.Builder<App.Model.Builder.DatasetBuilder>().Build(fields, data.ToModel());

			await this.ExecuteProfilingFlow(model);

			return id;
		}

		public async Task<Guid> FutureProfileAsync(Guid id)
		{
			this._logger.Debug(new MapLogEntry("future profiling").And("id", id));

			await this.AuthorizeProfileForce();
			await this.AuthorizeExecuteProfilingWorkflowForce();

			List<Dataset> datas = await this._queryFactory.Query<DatasetHttpQuery>().Ids(id).CollectAsync();
			if (datas == null || datas.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);
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
				nameof(App.Model.Dataset.DatePublished));
			App.Model.Dataset model = await this._builderFactory.Builder<App.Model.Builder.DatasetBuilder>().Build(fields, datas.First());

			await this.ExecuteFutureProfilingFlow(model);

			return id;
		}

		private async Task ExecuteProfilingFlow(App.Model.Dataset model)
		{
			this._logger.Debug(new MapLogEntry("executing").And("type", nameof(ExecuteProfilingFlow)).And("model", model));

			List<Airflow.Model.AirflowDag> definitions = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>()
				.Kinds(Common.WorkflowDefinitionKind.DatasetProfiling)
				.ExcludeStaled(true)
				.CollectAsync();

			if (definitions == null || definitions.Count != 1) throw new DGNotFoundException(this._localizer["general_notFound", Common.WorkflowDefinitionKind.DatasetProfiling.ToString(), nameof(App.Model.WorkflowDefinition)]);
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
					connector = DatasetConnectorType.RawDataPath.ToString(),
				}
			}, new FieldSet
			{
				Fields = [
				nameof(App.Model.WorkflowExecution.Id),
				nameof(App.Model.WorkflowExecution.WorkflowId),
				]
			});
		}

		private async Task ExecuteFutureProfilingFlow(App.Model.Dataset model)
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
					connector = DatasetConnectorType.RawDataPath.ToString(),
				}
			}, new FieldSet
			{
				Fields = [
				nameof(App.Model.WorkflowExecution.Id),
				nameof(App.Model.WorkflowExecution.WorkflowId),
				]
			});
		}

		public async Task<Guid> UpdateProfileAsDataManagementAsync(Guid id, String profile)
		{
			this._logger.Debug(new MapLogEntry("updating profile as data management").And("type", nameof(App.Model.DatasetPersist)).And("id", id).And("profile", profile));

			await this.AuthorizeEditForce(id);

			Data.Dataset data = await this._dbContext.Datasets.FindAsync(id);
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);

			data.Profile = profile;

			this._dbContext.Update(data);

			await this._dbContext.SaveChangesAsync();

			return id;
		}

		public async Task<App.Model.Dataset> PersistAsync(App.Model.DatasetPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.DatasetPersist)).And("model", model).And("fields", fields));

			await this.AuthorizeEditForce(model.Id.Value);

			Service.DataManagement.Model.Dataset data = await this.PatchAndSave(model, true);

			App.Model.Dataset persisted = await this._builderFactory.Builder<App.Model.Builder.DatasetBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.Dataset.Id)), data);
			return persisted;
		}

		private async Task<Service.DataManagement.Model.Dataset> PatchAndSave(App.Model.DatasetPersist model, Boolean isUpdate)
		{
			//model id always has vault. in case of edit or onboard
			Data.Dataset data = null;
			if (isUpdate)
			{
				data = await this._dbContext.Datasets.FindAsync(model.Id.Value);
				if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(App.Model.Dataset)]);
			}
			else
			{
				data = new Data.Dataset
				{
					Id = model.Id.Value,
				};
			}

			data.Name = model.Name;
			data.Code = model.Code;
			data.Description = model.Description;
			data.License = model.License;
			data.Url = model.Url;
			data.Version = model.Version;
			data.MimeType = model.MimeType;
			data.Size = model.Size;
			data.Headline = model.Headline;
			data.Keywords = model.Keywords == null ? null : String.Join(',', model.Keywords);
			data.FieldOfScience = model.FieldOfScience == null ? null : String.Join(',', model.FieldOfScience);
			data.Language = model.Language == null ? null : String.Join(',', model.Language);
			data.Country = model.Country == null ? null : String.Join(',', model.Country);
			data.DatePublished = model.DatePublished;

			if (isUpdate) this._dbContext.Update(data);
			else this._dbContext.Add(data);

			await this._dbContext.SaveChangesAsync();

			return data.ToModel();
		}

		public async Task DeleteAsync(Guid id)
		{
			await this.AuthorizDeleteForce(id);

			Data.Dataset data = await this._queryFactory.Query<DatasetLocalQuery>().Authorize(AuthorizationFlags.None).Ids(id).FirstAsync();
			if (data == null) return;

			List<Data.DatasetCollection> existingItems = await this._queryFactory.Query<Query.DatasetCollectionLocalQuery>().DatasetIds(id).Authorize(AuthorizationFlags.None).CollectAsync();
			this._dbContext.RemoveRange(existingItems);

			this._dbContext.Remove(data);

			await this._dbContext.SaveChangesAsync();

			await this._aaiService.DeleteDatasetGrants(data.Id);
			this._eventBroker.EmitDatasetDeleted(data.Id);
		}
	}
}
