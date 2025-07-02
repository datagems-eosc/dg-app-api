using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Query;
using Cite.Tools.Data.Deleter;

namespace DataGEMS.Gateway.App.Service.UserCollection
{
	public class UserCollectionService : IUserCollectionService
	{
		private readonly Data.AppDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IUserDatasetCollectionService _userDatasetCollectionService;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ILogger<UserCollectionService> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;

		public UserCollectionService(
			ILogger<UserCollectionService> logger,
			Data.AppDbContext dbContext,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			QueryFactory queryFactory,
			IUserDatasetCollectionService userDatasetCollectionService,
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
			this._userDatasetCollectionService = userDatasetCollectionService;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._localizer = localizer;
			this._errors = errors;
			this._eventBroker = eventBroker;
		}

		private async Task AuthorizeEditForce(Guid? userCollectionId)
		{
			await this.AuthorizeForce(userCollectionId, Permission.EditUserCollection);
		}

		private async Task AuthorizDeleteForce(Guid? userCollectionId)
		{
			await this.AuthorizeForce(userCollectionId, Permission.DeleteUserCollection);
		}

		private async Task AuthorizeForce(Guid? userCollectionId, String permission)
		{
			if (!userCollectionId.HasValue) return;

			Data.UserCollection data = await this._dbContext.UserCollections.FindAsync(userCollectionId);
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", userCollectionId.Value, nameof(Model.UserCollection)]);

			String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(data.UserId);
			await this._authorizationService.AuthorizeOrOwnerForce(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null, permission);
		}

		public async Task<Model.UserCollection> PersistAsync(Model.UserCollectionPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserCollectionPersist)).And("model", model).And("fields", fields));

			await this.AuthorizeEditForce(model.Id);

			Data.UserCollection data = await this.PatchAndSave(model);

			Model.UserCollection persisted = await this._builderFactory.Builder<Model.Builder.UserCollectionBuilder>().Build(FieldSet.Build(fields, nameof(Model.UserCollection.Id), nameof(Model.UserCollection.ETag)), data);
			return persisted;
		}

		public async Task<Model.UserCollection> PersistAsync(Model.UserCollectionPersistDeep model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserCollectionPersistDeep)).And("model", model).And("fields", fields));

			await this.AuthorizeEditForce(model.Id);

			Data.UserCollection data = await this.PatchAndSave(new Model.UserCollectionPersist()
			{
				Id = model.Id,
				Name = model.Name,
				ETag = model.ETag,
			});

			model.UserDatasetCollections?.ForEach(x => x.UserCollectionId = data.Id);
			await this.PatchAsync(new Model.UserCollectionDatasetPatch()
			{
				Id = data.Id,
				ETag = data.UpdatedAt.ToETag(),
				UserDatasetCollections = model.UserDatasetCollections
			});

			Model.UserCollection persisted = await this._builderFactory.Builder<Model.Builder.UserCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(FieldSet.Build(fields, nameof(Model.UserCollection.Id), nameof(Model.UserCollection.ETag)), data);
			return persisted;
		}

		private async Task<Data.UserCollection> PatchAndSave(Model.UserCollectionPersist model)
		{
			Boolean isUpdate = model.Id.HasValue && model.Id.Value != Guid.Empty;

			Data.UserCollection data = null;
			if (isUpdate)
			{
				data = await this._dbContext.UserCollections.FindAsync(model.Id.Value);
				if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(Model.UserCollection)]);
				if (!String.Equals(model.ETag, data.UpdatedAt.ToETag())) throw new DGValidationException(this._errors.ETagConflict.Code, string.Format(this._errors.ETagConflict.Message, data.Id, nameof(Data.UserCollection)));
			}
			else
			{
				Guid? userId = await this._authorizationContentResolver.CurrentUserId();
				if (!userId.HasValue) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
				data = new Data.UserCollection
				{
					Id = Guid.NewGuid(),
					IsActive = IsActive.Active,
					UserId = userId.Value,
					CreatedAt = DateTime.UtcNow
				};
			}

			if (isUpdate &&
				String.Equals(data.Name, model.Name)) return data;

			data.Name = model.Name;
			data.UpdatedAt = DateTime.UtcNow;

			if (isUpdate) this._dbContext.Update(data);
			else this._dbContext.Add(data);

			await this._dbContext.SaveChangesAsync();

			this._eventBroker.EmitUserCollectionTouched(data.Id);

			return data;
		}

		public async Task<Model.UserCollection> PatchAsync(Model.UserCollectionDatasetPatch model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserCollectionDatasetPatch)).And("model", model).And("fields", fields));

			Data.UserCollection data = await this._dbContext.UserCollections.FindAsync(model.Id.Value);
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(Model.UserCollection)]);
			if (!String.Equals(model.ETag, data.UpdatedAt.ToETag())) throw new DGValidationException(this._errors.ETagConflict.Code, string.Format(this._errors.ETagConflict.Message, data.Id, nameof(Data.UserCollection)));

			await this.AuthorizeEditForce(model.Id);

			List<Guid> existingItems = await this._queryFactory.Query<Query.UserDatasetCollectionQuery>().UserCollectionIds(model.Id.Value).IsActive(IsActive.Active).Authorize(AuthorizationFlags.Any).CollectAsync(x => x.Id);

			List<Guid> existingEditableItems = await this._userDatasetCollectionService.ApplyEditAccess(existingItems);
			HashSet<Guid> incomingUpdatingIds = model.UserDatasetCollections == null ? Enumerable.Empty<Guid>().ToHashSet() : model.UserDatasetCollections.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToHashSet();
			if (!incomingUpdatingIds.IsSubsetOf(existingEditableItems)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			List<Guid> existingDeletableItems = await this._userDatasetCollectionService.ApplyDeleteAccess(existingItems);
			HashSet<Guid> incomingDeletingIds = existingEditableItems.Except(incomingUpdatingIds).ToHashSet();
			if (!incomingDeletingIds.IsSubsetOf(existingDeletableItems)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			model.UserDatasetCollections = model.UserDatasetCollections?.Select(x => { x.UserCollectionId = data.Id; return x; })?.ToList();

			await this._userDatasetCollectionService.DeleteAsync(incomingDeletingIds);
			await this._userDatasetCollectionService.PersistAsync(model.UserDatasetCollections, null);

			Model.UserCollection persisted = await this._builderFactory.Builder<Model.Builder.UserCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(FieldSet.Build(fields, nameof(Model.UserCollection.Id), nameof(Model.UserCollection.ETag)), data);
			return persisted;
		}

		public async Task<Model.UserCollection> AddAsync(Guid userCollectionId, Guid datasetId, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("adding").And("userCollectionId", userCollectionId).And("datasetId", datasetId).And("fields", fields));

			await this.AuthorizeEditForce(userCollectionId);

			Boolean existing = await this._queryFactory.Query<Query.UserDatasetCollectionQuery>().UserCollectionIds(userCollectionId).DatasetIds(datasetId).IsActive(IsActive.Active).Authorize(AuthorizationFlags.None).AnyAsync();
			if (!existing) await this._userDatasetCollectionService.PersistAsync(new Model.UserDatasetCollectionPersist()
			{
				DatasetId = datasetId,
				UserCollectionId = userCollectionId,
			});

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(Model.UserCollection.Id), nameof(Model.UserCollection.ETag));
			Data.UserCollection data = await this._queryFactory.Query<UserCollectionQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(userCollectionId).FirstAsync(fieldsToUse);
			Model.UserCollection persisted = await this._builderFactory.Builder<Model.Builder.UserCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data);

			return persisted;
		}

		public async Task<Model.UserCollection> RemoveAsync(Guid userCollectionId, Guid datasetId, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("removing").And("userCollectionId", userCollectionId).And("datasetId", datasetId).And("fields", fields));

			await this.AuthorizeEditForce(userCollectionId);

			Guid userDatasetCollectionId = await this._queryFactory.Query<Query.UserDatasetCollectionQuery>().UserCollectionIds(userCollectionId).DatasetIds(datasetId).IsActive(IsActive.Active).Authorize(AuthorizationFlags.None).FirstAsync(x=> x.Id);
			if(userDatasetCollectionId == default(Guid)) throw new DGNotFoundException(this._localizer["general_notFound", $"{userCollectionId} - {datasetId}", nameof(Model.UserDatasetCollection)]);

			await this._userDatasetCollectionService.DeleteAsync(userDatasetCollectionId);

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(Model.UserCollection.Id), nameof(Model.UserCollection.ETag));
			Data.UserCollection data = await this._queryFactory.Query<UserCollectionQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(userCollectionId).FirstAsync(fieldsToUse);
			Model.UserCollection persisted = await this._builderFactory.Builder<Model.Builder.UserCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data);

			return persisted;
		}

		public async Task DeleteAsync(Guid id)
		{
			await this.AuthorizDeleteForce(id);

			await this._deleterFactory.Deleter<Deleter.UserCollectionDeleter>().DeleteAndSave([id]);
		}
	}
}
