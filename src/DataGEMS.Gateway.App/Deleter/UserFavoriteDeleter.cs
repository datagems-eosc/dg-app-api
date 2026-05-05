using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Deleter
{
	public class UserFavoriteDeleter : IDeleter
	{
		private readonly QueryFactory _queryFactory = null;
		private readonly DeleterFactory _deleterFactory = null;
		private readonly Data.AppDbContext _dbContext;
		private readonly ILogger<UserFavoriteDeleter> _logger;
		private readonly EventBroker _eventBroker;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;

		public UserFavoriteDeleter(
			Data.AppDbContext dbContext,
			QueryFactory queryFactory,
			DeleterFactory deleterFactory,
			EventBroker eventBroker,
			ILogger<UserFavoriteDeleter> logger,
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
			List<Data.UserFavorite> datas = await this._queryFactory.Query<UserFavoriteQuery>().Ids(ids).Authorize(Authorization.AuthorizationFlags.None).CollectAsync();
			await this.DeleteAndSave(datas);
		}

		public async Task DeleteAndSave(IEnumerable<Data.UserFavorite> datas)
		{
			await this.Delete(datas);
			await this._dbContext.SaveChangesAsync();
		}

		public async Task Delete(IEnumerable<Data.UserFavorite> datas)
		{
			this._logger.Debug(new MapLogEntry("deleting").And("type", nameof(App.Model.UserFavorite)).And("count", datas?.Count()));
			if (datas == null || !datas.Any()) return;

			List<Guid> ids = datas.Select(x => x.Id).Distinct().ToList();

			DateTime now = DateTime.UtcNow;
			foreach (Data.UserFavorite item in datas)
			{
				item.IsActive = IsActive.Inactive;
				this._dbContext.Update(item);
			}

			this._eventBroker.EmitUserFavoriteDeleted(datas.Select(x => x.Id).ToList());
		}
	}
}
