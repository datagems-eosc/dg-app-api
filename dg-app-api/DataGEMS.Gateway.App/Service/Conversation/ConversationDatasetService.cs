using Cite.Tools.Auth.Claims;
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Exception;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Common;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Service.Conversation
{
	public class ConversationDatasetService : IConversationDatasetService
	{
		private readonly Data.AppDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly ClaimExtractor _extractor;
		private readonly ILogger<ConversationDatasetService> _logger;
		private readonly ErrorThesaurus _errors;

		public ConversationDatasetService(
			ILogger<ConversationDatasetService> logger,
			Data.AppDbContext dbContext,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			ClaimExtractor extractor,
			IStringLocalizer<Resources.MySharedResources> localizer,
			ErrorThesaurus errors)
		{
			this._logger = logger;
			this._dbContext = dbContext;
			this._builderFactory = builderFactory;
			this._deleterFactory = deleterFactory;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._extractor = extractor;
			this._localizer = localizer;
			this._errors = errors;
		}

		public async Task<List<Guid>> ApplyEditAccess(List<Guid> ids)
		{
			this._logger.Debug(new MapLogEntry("access filtering").And("type", nameof(Model.ConversationDataset)).And("ids", ids).And("permission", Permission.EditConversationDataset));
			return await this.ApplyAccess(ids, Permission.EditConversationDataset, false);
		}

		public async Task<List<Guid>> ApplyDeleteAccess(List<Guid> ids)
		{
			this._logger.Debug(new MapLogEntry("access filtering").And("type", nameof(Model.ConversationDataset)).And("ids", ids).And("permission", Permission.DeleteConversationDataset));
			return await this.ApplyAccess(ids, Permission.DeleteConversationDataset, false);
		}

		private async Task<List<Guid>> ApplyAccess(IEnumerable<Guid> ids, String permission, Boolean force)
		{
			if (ids == null || !ids.Any()) return Enumerable.Empty<Guid>().ToList();

			var datas = this._dbContext.ConversationDatasets.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, UserId = x.Conversation.UserId });

			Guid? currentUser = this._extractor.SubjectGuid(this._currentPrincipalResolverService.CurrentPrincipal());

			HashSet<Guid> allowUserIds = new HashSet<Guid>();
			foreach (Guid userId in datas.Select(x => x.UserId).Distinct().ToList())
			{
				String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(userId);
				Boolean allow = await this._authorizationService.AuthorizeOrOwner(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null, permission);
				if (!allow && force) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
				if (allow) allowUserIds.Add(userId);
			}
			List<Guid> allowed = datas.Where(x => allowUserIds.Contains(x.UserId)).Select(x => x.Id).ToList();
			return allowed;
		}

		public async Task<Model.ConversationDataset> PersistAsync(Model.ConversationDatasetPersist model, IFieldSet fields = null)
		{
			List<Model.ConversationDataset> models = await this.PersistAsync(new Model.ConversationDatasetPersist[] { model }, fields);
			return models.FirstOrDefault();
		}

		public async Task<List<Model.ConversationDataset>> PersistAsync(IEnumerable<Model.ConversationDatasetPersist> models, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.ConversationDatasetPersist)).And("models", models).And("fields", fields));

			if (models == null || !models.Any()) return new List<Model.ConversationDataset>();

			Guid? currentUser = this._extractor.SubjectGuid(this._currentPrincipalResolverService.CurrentPrincipal());
			List<Guid> conversationIds = models.Where(x => x.ConversationId.HasValue).Select(x => x.ConversationId.Value).Distinct().ToList();
			if (conversationIds.Count == 0) return new List<Model.ConversationDataset>();
			List<Guid> userIds = this._dbContext.Conversations.Where(x => conversationIds.Contains(x.Id)).Select(x => x.UserId).Distinct().ToList();
			if (userIds.Count == 0) return new List<Model.ConversationDataset>();
			foreach (Guid userId in userIds)
			{
				String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(userId);
				await this._authorizationService.AuthorizeOrOwnerForce(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null, Permission.EditConversationDataset);
			}

			List<Guid> existingIds = models.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();
			List<Data.ConversationDataset> datas = await this._dbContext.ConversationDatasets.Where(x => existingIds.Contains(x.Id)).ToListAsync();

			List<Data.ConversationDataset> persistedData = new List<Data.ConversationDataset>();
			foreach (Model.ConversationDatasetPersist model in models)
			{
				Boolean isUpdate = model.Id.HasValue && model.Id.Value != Guid.Empty;

				Data.ConversationDataset data = null;
				if (isUpdate)
				{
					data = datas.FirstOrDefault(x => x.Id == model.Id.Value);
					if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(Model.ConversationDataset)]);
					if (!String.Equals(model.ETag, data.UpdatedAt.ToETag())) throw new DGValidationException(this._errors.ETagConflict.Code, string.Format(this._errors.ETagConflict.Message, data.Id, nameof(Data.ConversationDataset)));
					if (!Guid.Equals(data.ConversationId, model.ConversationId.Value)) throw new DGValidationException(this._errors.ImmutableItem.Code, this._errors.ImmutableItem.Message);
				}
				else
				{
					data = new Data.ConversationDataset
					{
						Id = Guid.NewGuid(),
						IsActive = IsActive.Active,
						CreatedAt = DateTime.UtcNow
					};
				}

				if (isUpdate &&
					data.ConversationId.Equals(model.ConversationId.Value) &&
					data.DatasetId.Equals(model.DatasetId.Value))
				{
					persistedData.Add(data);
					continue;
				}

				data.ConversationId = model.ConversationId.Value;
				data.DatasetId = model.DatasetId.Value;
				data.UpdatedAt = DateTime.UtcNow;

				if (isUpdate) this._dbContext.Update(data);
				else this._dbContext.Add(data);

				persistedData.Add(data);
			}

			await this._dbContext.SaveChangesAsync();

			List<Model.ConversationDataset> persisted = await this._builderFactory.Builder<Model.Builder.ConversationDatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(FieldSet.Build(fields, nameof(Model.ConversationDataset.Id), nameof(Model.ConversationDataset.ETag)), persistedData);
			return persisted;
		}

		public async Task DeleteAsync(Guid id)
		{
			await this.ApplyAccess([id], Permission.DeleteConversationDataset, true);

			await this._deleterFactory.Deleter<Deleter.ConversationDatasetDeleter>().DeleteAndSave([id]);
		}

		public async Task DeleteAsync(IEnumerable<Guid> ids)
		{
			await this.ApplyAccess(ids, Permission.DeleteConversationDataset, true);

			await this._deleterFactory.Deleter<Deleter.ConversationDatasetDeleter>().DeleteAndSave(ids);
		}
	}
}
