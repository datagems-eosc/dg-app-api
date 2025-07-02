using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Deleter
{
	public class ConversationDeleter : IDeleter
	{
		private readonly QueryFactory _queryFactory = null;
		private readonly DeleterFactory _deleterFactory = null;
		private readonly Data.AppDbContext _dbContext;
		private readonly ILogger<ConversationDeleter> _logger;
		private readonly EventBroker _eventBroker;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;

		public ConversationDeleter(
			Data.AppDbContext dbContext,
			QueryFactory queryFactory,
			DeleterFactory deleterFactory,
			EventBroker eventBroker,
			ILogger<ConversationDeleter> logger,
			ErrorThesaurus errors,
			IStringLocalizer<Resources.MySharedResources> localizer)
		{
			this._logger = logger;
			this._dbContext = dbContext;
			this._queryFactory = queryFactory;
			this._deleterFactory = deleterFactory;
			this._eventBroker = eventBroker;
			this._errors = errors;
			this._localizer = localizer;
		}

		public async Task DeleteAndSave(IEnumerable<Guid> ids)
		{
			List<Data.Conversation> datas = await this._queryFactory.Query<ConversationQuery>().Ids(ids).Authorize(Authorization.AuthorizationFlags.None).CollectAsync();
			await this.DeleteAndSave(datas);
		}

		public async Task DeleteAndSave(IEnumerable<Data.Conversation> datas)
		{
			await this.Delete(datas);
			await this._dbContext.SaveChangesAsync();
		}

		public async Task Delete(IEnumerable<Data.Conversation> datas)
		{
			this._logger.Debug(new MapLogEntry("deleting").And("type", nameof(App.Model.Conversation)).And("count", datas?.Count()));
			if (datas == null || !datas.Any()) return;

			List<Guid> ids = datas.Select(x => x.Id).Distinct().ToList();
			List<Data.ConversationDataset> conversationDatasets = await this._queryFactory.Query<ConversationDatasetQuery>()
				.ConversationIds(ids)
				.IsActive(IsActive.Active)
				.Authorize(Authorization.AuthorizationFlags.None)
				.CollectAsync();
			await this._deleterFactory.Deleter<ConversationDatasetDeleter>().Delete(conversationDatasets);

			List<Data.ConversationMessage> conversationMessages = await this._queryFactory.Query<ConversationMessageQuery>()
				.ConversationIds(ids)
				.Authorize(Authorization.AuthorizationFlags.None)
				.CollectAsync();
			await this._deleterFactory.Deleter<ConversationMessageDeleter>().Delete(conversationMessages);

			DateTime now = DateTime.UtcNow;
			foreach (Data.Conversation item in datas)
			{
				item.IsActive = IsActive.Inactive;
				item.UpdatedAt = now;
				this._dbContext.Update(item);
			}

			this._eventBroker.EmitConversationDeleted(datas.Select(x => x.Id).ToList());
		}
	}
}
