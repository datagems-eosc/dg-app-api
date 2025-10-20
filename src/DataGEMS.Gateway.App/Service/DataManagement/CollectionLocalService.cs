using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common.Auth;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.AAI;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
	public class CollectionLocalService : ICollectionService
	{
		private readonly Data.DataManagementDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ILogger<CollectionLocalService> _logger;
		private readonly AAIConfig _aaiConfig;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;
		private readonly IAAIService _aaiService;

		public CollectionLocalService(
			ILogger<CollectionLocalService> logger,
			Data.DataManagementDbContext dbContext,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			QueryFactory queryFactory,
			IAAIService aaiService,
			AAIConfig aaiConfig,
			IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
			IStringLocalizer<Resources.MySharedResources> localizer,
			ErrorThesaurus errors,
			EventBroker eventBroker)
		{
			this._logger = logger;
			this._dbContext = dbContext;
			this._builderFactory = builderFactory;
			this._deleterFactory = deleterFactory;
			this._queryFactory = queryFactory;
			this._aaiService = aaiService;
			this._aaiConfig = aaiConfig;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._localizer = localizer;
			this._errors = errors;
			this._eventBroker = eventBroker;
		}

		private async Task AuthorizeCreateForce()
		{
			await this._authorizationService.AuthorizeForce(Permission.CreateCollection);
		}

		private async Task AuthorizeEditForce(Guid collectionId)
		{
			await this.AuthorizeForce(collectionId, Permission.EditCollection);
		}

		private async Task AuthorizDeleteForce(Guid collectionId)
		{
			await this.AuthorizeForce(collectionId, Permission.DeleteCollection);
		}

		private async Task AuthorizeForce(Guid collectionId, String permission)
		{
			HashSet<string> userDatasetGroupRoles = await _authorizationContentResolver.ContextRolesForCollection(collectionId);
			await this._authorizationService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(userDatasetGroupRoles), permission);
		}

		public async Task<App.Model.Collection> PersistAsync(App.Model.CollectionPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.CollectionPersist)).And("model", model).And("fields", fields));

			if (!model.Id.HasValue) await this.AuthorizeCreateForce();
			else await this.AuthorizeEditForce(model.Id.Value);

			Service.DataManagement.Model.Collection data = await this.PatchAndSave(model);

			List<Common.Auth.ContextGrant> contextGrants = await this._aaiService.BootstrapContextGrantGroupsFor(Common.Auth.ContextGrant.TargetType.Group, data.Id.ToString().ToLowerInvariant());
			if (!model.Id.HasValue && this._aaiConfig.AutoAssignGrantsOnNewCollection != null && this._aaiConfig.AutoAssignGrantsOnNewCollection.Count > 0)
			{
				String subjectId = await this._authorizationContentResolver.SubjectIdOfCurrentUser();
				List<String> autoAssignGroups = contextGrants.Where(x => this._aaiConfig.AutoAssignGrantsOnNewCollection.Contains(x.Access)).Select(x => x.GroupId).ToList();
				foreach (String groupId in autoAssignGroups)
				{
					await this._aaiService.AddUserToContextGrantGroup(subjectId, groupId);
				}
				this._eventBroker.EmitUserDatasetGrantTouched(subjectId);
			}
			this._eventBroker.EmitCollectionTouched(data.Id);

			App.Model.Collection persisted = await this._builderFactory.Builder<App.Model.Builder.CollectionBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.Collection.Id)), data);
			return persisted;
		}

		public async Task<App.Model.Collection> PersistAsync(App.Model.CollectionPersistDeep model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.CollectionPersistDeep)).And("model", model).And("fields", fields));

			if (!model.Id.HasValue) await this.AuthorizeCreateForce();
			else await this.AuthorizeEditForce(model.Id.Value);

			Service.DataManagement.Model.Collection data = await this.PatchAndSave(new App.Model.CollectionPersist()
			{
				Id = model.Id,
				Name = model.Name,
				Code = model.Code
			});

			await this.PatchAsync(new App.Model.CollectionDatasetPatch()
			{
				Id = data.Id,
				Datasets = model.Datasets
			});

			List<Common.Auth.ContextGrant> contextGrants = await this._aaiService.BootstrapContextGrantGroupsFor(Common.Auth.ContextGrant.TargetType.Group, data.Id.ToString().ToLowerInvariant());
			if (!model.Id.HasValue && this._aaiConfig.AutoAssignGrantsOnNewCollection != null && this._aaiConfig.AutoAssignGrantsOnNewCollection.Count > 0)
			{
				String subjectId = await this._authorizationContentResolver.SubjectIdOfCurrentUser();
				List<String> autoAssignGroups = contextGrants.Where(x => this._aaiConfig.AutoAssignGrantsOnNewCollection.Contains(x.Access)).Select(x => x.GroupId).ToList();
				foreach(String groupId in autoAssignGroups)
				{
					await this._aaiService.AddUserToContextGrantGroup(subjectId, groupId);
				}
				this._eventBroker.EmitUserDatasetGrantTouched(subjectId);
			}
			this._eventBroker.EmitCollectionTouched(data.Id);

			App.Model.Collection persisted = await this._builderFactory.Builder<App.Model.Builder.CollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(FieldSet.Build(fields, nameof(App.Model.Collection.Id)), data);
			return persisted;
		}

		private async Task<Service.DataManagement.Model.Collection> PatchAndSave(App.Model.CollectionPersist model)
		{
			Boolean isUpdate = model.Id.HasValue && model.Id.Value != Guid.Empty;

			Data.Collection data = null;
			if (isUpdate)
			{
				data = await this._dbContext.Collections.FindAsync(model.Id.Value);
				if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(App.Model.Collection)]);
			}
			else
			{
				data = new Data.Collection
				{
					Id = Guid.NewGuid(),
				};
			}

			if (isUpdate &&
				String.Equals(data.Name, model.Name) &&
				String.Equals(data.Code, model.Code)) return data.ToModel();

			data.Name = model.Name;
			data.Code = model.Code;

			if (isUpdate) this._dbContext.Update(data);
			else this._dbContext.Add(data);

			await this._dbContext.SaveChangesAsync();

			return data.ToModel();
		}

		public async Task<App.Model.Collection> PatchAsync(App.Model.CollectionDatasetPatch model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.CollectionDatasetPatch)).And("model", model).And("fields", fields));

			Data.Collection data = await this._dbContext.Collections.FindAsync(model.Id.Value);
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(Model.Collection)]);

			await this.AuthorizeEditForce(model.Id.Value);

			List<Data.DatasetCollection> existingItems = await this._queryFactory.Query<Query.DatasetCollectionLocalQuery>().CollectionIds(model.Id.Value).Authorize(AuthorizationFlags.Any).CollectAsync();
			this._dbContext.RemoveRange(existingItems);

			List<Data.DatasetCollection> newItems = model.Datasets.Select(x => new Data.DatasetCollection()
			{
				Id = Guid.NewGuid(),
				CollectionId = model.Id.Value,
				DatasetId = x
			}).ToList();

			this._dbContext.AddRange(newItems);

			await this._dbContext.SaveChangesAsync();

			this._eventBroker.EmitCollectionTouched(data.Id);

			App.Model.Collection persisted = await this._builderFactory.Builder<App.Model.Builder.CollectionBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.Collection.Id)), data.ToModel());
			return persisted;
		}

		public async Task<App.Model.Collection> AddAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("adding").And("collectionId", collectionId).And("datasetId", datasetId).And("fields", fields));

			await this.AuthorizeEditForce(collectionId);

			Boolean existing = await this._queryFactory.Query<Query.DatasetCollectionLocalQuery>().CollectionIds(collectionId).DatasetIds(datasetId).Authorize(AuthorizationFlags.None).AnyAsync();
			if (!existing)
			{
				this._dbContext.Add(new Data.DatasetCollection()
				{
					Id = Guid.NewGuid(),
					CollectionId = collectionId,
					DatasetId = datasetId
				});

				await this._dbContext.SaveChangesAsync();
			}

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(App.Model.Collection.Id));
			Data.Collection data = await this._queryFactory.Query<CollectionLocalQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(collectionId).FirstAsync(fieldsToUse);
			App.Model.Collection persisted = await this._builderFactory.Builder<App.Model.Builder.CollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data.ToModel());

			this._eventBroker.EmitCollectionTouched(data.Id);

			return persisted;
		}

		public async Task<App.Model.Collection> RemoveAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("removing").And("collectionId", collectionId).And("datasetId", datasetId).And("fields", fields));

			await this.AuthorizeEditForce(collectionId);

			Data.DatasetCollection existing = await this._queryFactory.Query<Query.DatasetCollectionLocalQuery>().CollectionIds(collectionId).DatasetIds(datasetId).Authorize(AuthorizationFlags.None).FirstAsync();
			if (existing == null) throw new DGNotFoundException(this._localizer["general_notFound", $"{collectionId} - {datasetId}", nameof(App.Model.Dataset)]);

			this._dbContext.Remove(existing);
			await this._dbContext.SaveChangesAsync();

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(App.Model.Collection.Id));
			Data.Collection data = await this._queryFactory.Query<CollectionLocalQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(collectionId).FirstAsync(fieldsToUse);
			App.Model.Collection persisted = await this._builderFactory.Builder<App.Model.Builder.CollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data.ToModel());

			this._eventBroker.EmitCollectionTouched(data.Id);

			return persisted;
		}

		public async Task DeleteAsync(Guid id)
		{
			await this.AuthorizDeleteForce(id);

			Data.Collection data = await this._queryFactory.Query<CollectionLocalQuery>().Authorize(AuthorizationFlags.None).Ids(id).FirstAsync();
			if (data == null) return;

			List<Data.DatasetCollection> existingItems = await this._queryFactory.Query<Query.DatasetCollectionLocalQuery>().CollectionIds(id).Authorize(AuthorizationFlags.None).CollectAsync();
			this._dbContext.RemoveRange(existingItems);

			this._dbContext.Remove(data);

			await this._dbContext.SaveChangesAsync();

			await this._aaiService.DeleteContextGrantGroupsFor(data.Id.ToString().ToLowerInvariant());
			this._eventBroker.EmitCollectionDeleted(data.Id);
		}
	}
}
