using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Query;

namespace DataGEMS.Gateway.App.Service.Conversation
{
	public class ConversationService : IConversationService
	{
		private readonly Data.AppDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IConversationDatasetService _conversationDatasetService;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ILogger<ConversationService> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;

		public ConversationService(
			ILogger<ConversationService> logger,
			Data.AppDbContext dbContext,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			QueryFactory queryFactory,
			IConversationDatasetService conversationDatasetService,
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
			this._conversationDatasetService = conversationDatasetService;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._localizer = localizer;
			this._errors = errors;
			this._eventBroker = eventBroker;
		}

		private async Task AuthorizeEditForce(Guid? conversationId)
		{
			await this.AuthorizeForce(conversationId, Permission.EditConversation);
		}

		private async Task AuthorizDeleteForce(Guid? conversationId)
		{
			await this.AuthorizeForce(conversationId, Permission.DeleteConversation);
		}

		private async Task AuthorizeForce(Guid? conversationId, String permission)
		{
			if (!conversationId.HasValue) return;

			Data.Conversation data = await this._dbContext.Conversations.FindAsync(conversationId);
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", conversationId.Value, nameof(Model.Conversation)]);

			String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(data.UserId);
			await this._authorizationService.AuthorizeOrOwnerForce(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null, permission);
		}

		public async Task<Model.Conversation> PersistAsync(Model.ConversationPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.ConversationPersist)).And("model", model).And("fields", fields));

			await this.AuthorizeEditForce(model.Id);

			Data.Conversation data = await this.PatchAndSave(model);

			Model.Conversation persisted = await this._builderFactory.Builder<Model.Builder.ConversationBuilder>().Build(FieldSet.Build(fields, nameof(Model.Conversation.Id), nameof(Model.Conversation.ETag)), data);
			return persisted;
		}

		private async Task<Data.Conversation> PatchAndSave(Model.ConversationPersist model)
		{
			Boolean isUpdate = model.Id.HasValue && model.Id.Value != Guid.Empty;

			Data.Conversation data = null;
			if (isUpdate)
			{
				data = await this._dbContext.Conversations.FindAsync(model.Id.Value);
				if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(Model.Conversation)]);
				if (!String.Equals(model.ETag, data.UpdatedAt.ToETag())) throw new DGValidationException(this._errors.ETagConflict.Code, string.Format(this._errors.ETagConflict.Message, data.Id, nameof(Data.Conversation)));
			}
			else
			{
				Guid? userId = await this._authorizationContentResolver.CurrentUserId();
				if (!userId.HasValue) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
				data = new Data.Conversation
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

			this._eventBroker.EmitConversationTouched(data.Id);

			return data;
		}

		public async Task<Model.Conversation> AddAsync(Guid conversationId, Guid datasetId, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("adding").And("conversationId", conversationId).And("datasetId", datasetId).And("fields", fields));

			await this.AuthorizeEditForce(conversationId);

			Boolean existing = await this._queryFactory.Query<Query.ConversationDatasetQuery>().ConversationIds(conversationId).DatasetIds(datasetId).IsActive(IsActive.Active).Authorize(AuthorizationFlags.None).AnyAsync();
			if (!existing) await this._conversationDatasetService.PersistAsync(new Model.ConversationDatasetPersist()
			{
				DatasetId = datasetId,
				ConversationId = conversationId,
			});

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(Model.Conversation.Id), nameof(Model.Conversation.ETag));
			Data.Conversation data = await this._queryFactory.Query<ConversationQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(conversationId).FirstAsync(fieldsToUse);
			Model.Conversation persisted = await this._builderFactory.Builder<Model.Builder.ConversationBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data);

			return persisted;
		}

		public async Task<Model.Conversation> RemoveAsync(Guid conversationId, Guid datasetId, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("removing").And("conversationId", conversationId).And("datasetId", datasetId).And("fields", fields));

			await this.AuthorizeEditForce(conversationId);

			Guid conversationDatasetId = await this._queryFactory.Query<Query.ConversationDatasetQuery>().ConversationIds(conversationId).DatasetIds(datasetId).IsActive(IsActive.Active).Authorize(AuthorizationFlags.None).FirstAsync(x => x.Id);
			if (conversationDatasetId == default(Guid)) throw new DGNotFoundException(this._localizer["general_notFound", $"{conversationId} - {datasetId}", nameof(Model.ConversationDataset)]);

			await this._conversationDatasetService.DeleteAsync(conversationDatasetId);

			IFieldSet fieldsToUse = FieldSet.Build(fields, nameof(Model.Conversation.Id), nameof(Model.Conversation.ETag));
			Data.Conversation data = await this._queryFactory.Query<ConversationQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).Ids(conversationId).FirstAsync(fieldsToUse);
			Model.Conversation persisted = await this._builderFactory.Builder<Model.Builder.ConversationBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldsToUse, data);

			return persisted;
		}

		public async Task DeleteAsync(Guid id)
		{
			await this.AuthorizDeleteForce(id);

			await this._deleterFactory.Deleter<Deleter.ConversationDeleter>().DeleteAndSave([id]);
		}
	}
}
