using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Data;
using DataGEMS.Gateway.App.Deleter;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.AAI;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Service.Collection
{
	public class CollectionService : ICollectionService
	{
		private readonly Data.AppDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ILogger<CollectionService> _logger;
		private readonly AAIConfig _aaiConfig;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;
		private readonly IAAIService _aaiService;

		public CollectionService(
			ILogger<CollectionService> logger,
			Data.AppDbContext dbContext,
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
			_logger = logger;
			_dbContext = dbContext;
			_builderFactory = builderFactory;
			_deleterFactory = deleterFactory;
			_queryFactory = queryFactory;
			_aaiService = aaiService;
			_aaiConfig = aaiConfig;
			_authorizationService = authorizationService;
			_authorizationContentResolver = authorizationContentResolver;
			_localizer = localizer;
			_errors = errors;
			_eventBroker = eventBroker;
		}

		private async Task AuthorizeCreateForce()
		{
			await _authorizationService.AuthorizeForce(Permission.CreateCollection);
		}

		private async Task AuthorizeEditForce(Guid collectionId)
		{
			await AuthorizeForce(collectionId, Permission.EditCollection);
		}

		private async Task AuthorizDeleteForce(Guid collectionId)
		{
			await AuthorizeForce(collectionId, Permission.DeleteCollection);
		}

		private async Task AuthorizeForce(Guid collectionId, string permission)
		{
			HashSet<string> userDatasetGroupRoles = await _authorizationContentResolver.ContextRolesForCollectionOfUser(collectionId);
			await _authorizationService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(userDatasetGroupRoles), permission);
		}

		private async Task AutoAssignNewCollectionRoles(Guid collectionId)
		{
			if (_aaiConfig.AutoAssignGrantsOnNewCollection == null || _aaiConfig.AutoAssignGrantsOnNewCollection.Count == 0) return;

			string subjectId = await _authorizationContentResolver.SubjectIdOfCurrentUser();
			await _aaiService.BootstrapUserContextGrants(subjectId);
			await _aaiService.AssignCollectionGrantToUser(subjectId, collectionId, _aaiConfig.AutoAssignGrantsOnNewCollection);
		}

		public async Task<Model.Collection> PersistAsync(Model.CollectionPersist model, IFieldSet fields = null)
		{
			_logger.Debug(new MapLogEntry("persisting").And("type", nameof(Model.CollectionPersist)).And("model", model).And("fields", fields));

			if (!model.Id.HasValue) await AuthorizeCreateForce();
			else await AuthorizeEditForce(model.Id.Value);

			Data.Collection data = await PatchAndSave(model);

			if (!model.Id.HasValue) await AutoAssignNewCollectionRoles(data.Id);
			_eventBroker.EmitCollectionTouched(data.Id);

			Model.Collection persisted = await _builderFactory.Builder<Model.Builder.CollectionBuilder>().Build(FieldSet.Build(fields, nameof(Model.Collection.Id)), data);
			return persisted;
		}

		public async Task<Model.Collection> PersistAsync(Model.CollectionPersistDeep model, IFieldSet fields = null)
		{
			_logger.Debug(new MapLogEntry("persisting").And("type", nameof(Model.CollectionPersistDeep)).And("model", model).And("fields", fields));

			if (!model.Id.HasValue) await AuthorizeCreateForce();
			else await AuthorizeEditForce(model.Id.Value);

			Data.Collection data = await PatchAndSave(new Model.CollectionPersist()
			{
				Id = model.Id,
				Name = model.Name,
				Code = model.Code
			});

			await PatchAsync(new Model.CollectionDatasetPatch()
			{
				Id = data.Id,
				Datasets = model.Datasets
			});

			if (!model.Id.HasValue) await AutoAssignNewCollectionRoles(data.Id);
			_eventBroker.EmitCollectionTouched(data.Id);

			Model.Collection persisted = await _builderFactory.Builder<Model.Builder.CollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(FieldSet.Build(fields, nameof(Model.Collection.Id)), data);
			return persisted;
		}

		private async Task<Data.Collection> PatchAndSave(Model.CollectionPersist model)
		{
			bool isUpdate = model.Id.HasValue && model.Id.Value != Guid.Empty;

			Data.Collection data = null;
			if (isUpdate)
			{
				data = await _dbContext.Collections.FindAsync(model.Id.Value);
				if (data == null) throw new DGNotFoundException(_localizer["general_notFound", model.Id.Value, nameof(Model.Collection)]);
			}
			else
			{
				data = new Data.Collection
				{
					Id = Guid.NewGuid(),
				};
			}

			if (isUpdate &&
				string.Equals(data.Name, model.Name) &&
				string.Equals(data.Code, model.Code)) return data;

			data.Name = model.Name;
			data.Code = model.Code;

			if (isUpdate) _dbContext.Update(data);
			else _dbContext.Add(data);

			await _dbContext.SaveChangesAsync();

			return data;
		}

		public async Task<Model.Collection> PatchAsync(Model.CollectionDatasetPatch model, IFieldSet fields = null)
		{
			_logger.Debug(new MapLogEntry("persisting").And("type", nameof(Model.CollectionDatasetPatch)).And("model", model).And("fields", fields));

			Data.Collection data = await _dbContext.Collections.FindAsync(model.Id.Value);
			if (data == null) throw new DGNotFoundException(_localizer["general_notFound", model.Id.Value, nameof(Model.Collection)]);

			await AuthorizeEditForce(model.Id.Value);

			List<DatasetCollection> existingItems = await _queryFactory.Query<DatasetCollectionQuery>().CollectionIds(model.Id.Value).Authorize(AuthorizationFlags.Any).CollectAsync();
			await this._deleterFactory.Deleter<DatasetCollectionDeleter>().Delete(existingItems);

			List<DatasetCollection> newItems = model.Datasets.Select(x => new DatasetCollection()
			{
				Id = Guid.NewGuid(),
				CollectionId = model.Id.Value,
				DatasetId = x
			}).ToList();

			_dbContext.AddRange(newItems);

			await _dbContext.SaveChangesAsync();

			_eventBroker.EmitDatasetCollectionTouched(newItems.Select(x => new OnDatasetCollectionEventArgs.DatasetCollectionIdentifier() { CollectionId = x.CollectionId, DatasetId = x.DatasetId }).ToList());
			_eventBroker.EmitCollectionTouched(data.Id);

			Model.Collection persisted = await _builderFactory.Builder<Model.Builder.CollectionBuilder>().Build(FieldSet.Build(fields, nameof(Model.Collection.Id)), data);
			return persisted;
		}

		public async Task<Model.Collection> AddAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null)
		{
			_logger.Debug(new MapLogEntry("adding").And("collectionId", collectionId).And("datasetId", datasetId).And("fields", fields));

			await AuthorizeEditForce(collectionId);

			bool existing = await _queryFactory.Query<DatasetCollectionQuery>().CollectionIds(collectionId).DatasetIds(datasetId).Authorize(AuthorizationFlags.None).AnyAsync();
			if (!existing)
			{
				_dbContext.Add(new DatasetCollection()
				{
					Id = Guid.NewGuid(),
					CollectionId = collectionId,
					DatasetId = datasetId
				});

				await _dbContext.SaveChangesAsync();

				_eventBroker.EmitDatasetCollectionTouched(new OnDatasetCollectionEventArgs.DatasetCollectionIdentifier() { CollectionId = collectionId, DatasetId = datasetId });
			}

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(Model.Collection.Id));
			Data.Collection data = await _queryFactory.Query<CollectionQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(collectionId).FirstAsync(fieldsToUse);
			Model.Collection persisted = await _builderFactory.Builder<Model.Builder.CollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data);

			_eventBroker.EmitCollectionTouched(data.Id);

			return persisted;
		}

		public async Task<Model.Collection> RemoveAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null)
		{
			_logger.Debug(new MapLogEntry("removing").And("collectionId", collectionId).And("datasetId", datasetId).And("fields", fields));

			await AuthorizeEditForce(collectionId);

			DatasetCollection existing = await _queryFactory.Query<DatasetCollectionQuery>().CollectionIds(collectionId).DatasetIds(datasetId).Authorize(AuthorizationFlags.None).FirstAsync();
			if (existing == null) throw new DGNotFoundException(_localizer["general_notFound", $"{collectionId} - {datasetId}", nameof(Model.Dataset)]);

			await this._deleterFactory.Deleter<DatasetCollectionDeleter>().Delete([existing]);

			await _dbContext.SaveChangesAsync();

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(Model.Collection.Id));
			Data.Collection data = await _queryFactory.Query<CollectionQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(collectionId).FirstAsync(fieldsToUse);
			Model.Collection persisted = await _builderFactory.Builder<Model.Builder.CollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data);

			_eventBroker.EmitCollectionTouched(data.Id);

			return persisted;
		}

		public async Task DeleteAsync(Guid id)
		{
			await AuthorizDeleteForce(id);

			await this._deleterFactory.Deleter<Deleter.CollectionDeleter>().DeleteAndSave([id]);

			await _aaiService.DeleteCollectionGrants(id);
		}
	}
}
