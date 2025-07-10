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
	public class UserDatasetCollectionDeleter : IDeleter
	{
		private readonly QueryFactory _queryFactory = null;
		private readonly Data.AppDbContext _dbContext;
		private readonly ILogger<UserDatasetCollectionDeleter> _logger;
		private readonly EventBroker _eventBroker;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;

		public UserDatasetCollectionDeleter(
			Data.AppDbContext dbContext,
			QueryFactory queryFactory,
			EventBroker eventBroker,
			ILogger<UserDatasetCollectionDeleter> logger,
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
			List<Data.UserDatasetCollection> datas = await this._queryFactory.Query<UserDatasetCollectionQuery>().Ids(ids).Authorize(Authorization.AuthorizationFlags.None).CollectAsync();
			await this.DeleteAndSave(datas);
		}

		public async Task DeleteAndSave(IEnumerable<Data.UserDatasetCollection> datas)
		{
			await this.Delete(datas);
			await this._dbContext.SaveChangesAsync();
		}

		public Task Delete(IEnumerable<Data.UserDatasetCollection> datas)
		{
			this._logger.Debug(new MapLogEntry("deleting").And("type", nameof(App.Model.UserDatasetCollection)).And("count", datas?.Count()));
			if (datas == null || !datas.Any()) return Task.CompletedTask;

			DateTime now = DateTime.UtcNow;

			foreach (Data.UserDatasetCollection item in datas)
			{
				item.IsActive = IsActive.Inactive;
				item.UpdatedAt = now;
				this._dbContext.Update(item);
			}

			this._eventBroker.EmitUserDatasetCollectionDeleted(datas.Select(x => x.Id).ToList());
			return Task.CompletedTask;
		}
	}
}
