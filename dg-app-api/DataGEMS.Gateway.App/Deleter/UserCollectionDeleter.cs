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
	public class UserCollectionDeleter : IDeleter
	{
		private readonly QueryFactory _queryFactory = null;
		private readonly DeleterFactory _deleterFactory = null;
		private readonly Data.AppDbContext _dbContext;
		private readonly ILogger<UserCollectionDeleter> _logger;
		private readonly EventBroker _eventBroker;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;

		public UserCollectionDeleter(
			Data.AppDbContext dbContext,
			QueryFactory queryFactory,
			DeleterFactory deleterFactory,
			EventBroker eventBroker,
			ILogger<UserCollectionDeleter> logger,
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
			List<Data.UserCollection> datas = await this._queryFactory.Query<UserCollectionQuery>().Ids(ids).Authorize(Authorization.AuthorizationFlags.None).CollectAsync();
			await this.DeleteAndSave(datas);
		}

		public async Task DeleteAndSave(IEnumerable<Data.UserCollection> datas)
		{
			await this.Delete(datas);
			await this._dbContext.SaveChangesAsync();
		}

		public async Task Delete(IEnumerable<Data.UserCollection> datas)
		{
			this._logger.Debug(new MapLogEntry("deleting").And("type", nameof(App.Model.UserCollection)).And("count", datas?.Count()));
			if (datas == null || !datas.Any()) return;

			List<Guid> ids = datas.Select(x => x.Id).Distinct().ToList();
			List<Data.UserDatasetCollection> userDatasetCollections = await this._queryFactory.Query<UserDatasetCollectionQuery>()
				.UserCollectionIds(ids)
				.IsActive(IsActive.Active)
				.Authorize(Authorization.AuthorizationFlags.None)
				.CollectAsync();

			await this._deleterFactory.Deleter<UserDatasetCollectionDeleter>().Delete(userDatasetCollections);

			DateTime now = DateTime.UtcNow;
			foreach (Data.UserCollection item in datas)
			{
				item.IsActive = IsActive.Inactive;
				item.UpdatedAt = now;
				this._dbContext.Update(item);
			}

			this._eventBroker.EmitUserCollectionDeleted(datas.Select(x => x.Id).ToList());
		}
	}
}
