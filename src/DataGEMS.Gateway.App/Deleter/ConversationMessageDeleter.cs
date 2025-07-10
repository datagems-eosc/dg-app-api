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
	public class ConversationMessageDeleter : IDeleter
	{
		private readonly QueryFactory _queryFactory = null;
		private readonly Data.AppDbContext _dbContext;
		private readonly ILogger<ConversationMessageDeleter> _logger;
		private readonly EventBroker _eventBroker;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;

		public ConversationMessageDeleter(
			Data.AppDbContext dbContext,
			QueryFactory queryFactory,
			EventBroker eventBroker,
			ILogger<ConversationMessageDeleter> logger,
			ErrorThesaurus errors,
			IStringLocalizer<Resources.MySharedResources> localizer)
		{
			this._logger = logger;
			this._dbContext = dbContext;
			this._queryFactory = queryFactory;
			this._eventBroker = eventBroker;
			this._errors = errors;
			this._localizer = localizer;
		}

		public async Task DeleteAndSave(IEnumerable<Guid> ids)
		{
			List<Data.ConversationMessage> datas = await this._queryFactory.Query<ConversationMessageQuery>().Ids(ids).Authorize(Authorization.AuthorizationFlags.None).CollectAsync();
			await this.DeleteAndSave(datas);
		}

		public async Task DeleteAndSave(IEnumerable<Data.ConversationMessage> datas)
		{
			await this.Delete(datas);
			await this._dbContext.SaveChangesAsync();
		}

		public Task Delete(IEnumerable<Data.ConversationMessage> datas)
		{
			this._logger.Debug(new MapLogEntry("deleting").And("type", nameof(App.Model.ConversationMessage)).And("count", datas?.Count()));
			if (datas == null || !datas.Any()) return Task.CompletedTask;

			this._dbContext.RemoveRange(datas);

			this._eventBroker.EmitConversationMessageDeleted(datas.Select(x => x.ConversationId).Distinct().ToList());
			return Task.CompletedTask;
		}
	}
}
