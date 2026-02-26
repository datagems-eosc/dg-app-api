using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Deleter
{
	public class CollectionDeleter : IDeleter
	{
		private readonly QueryFactory _queryFactory = null;
		private readonly DeleterFactory _deleterFactory = null;
		private readonly Data.AppDbContext _dbContext;
		private readonly ILogger<CollectionDeleter> _logger;
		private readonly EventBroker _eventBroker;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;

		public CollectionDeleter(
			Data.AppDbContext dbContext,
			QueryFactory queryFactory,
			DeleterFactory deleterFactory,
			EventBroker eventBroker,
			ILogger<CollectionDeleter> logger,
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
			List<Data.Collection> datas = await this._queryFactory.Query<CollectionQuery>().Ids(ids).Authorize(Authorization.AuthorizationFlags.None).CollectAsync();
			await this.DeleteAndSave(datas);
		}

		public async Task DeleteAndSave(IEnumerable<Data.Collection> datas)
		{
			await this.Delete(datas);
			await this._dbContext.SaveChangesAsync();
		}

		public async Task Delete(IEnumerable<Data.Collection> datas)
		{
			this._logger.Debug(new MapLogEntry("deleting").And("type", nameof(App.Model.Collection)).And("count", datas?.Count()));
			if (datas == null || !datas.Any()) return;

			List<Guid> ids = datas.Select(x => x.Id).Distinct().ToList();
			List<Data.DatasetCollection> datasetCollections = await this._queryFactory.Query<DatasetCollectionQuery>()
				.CollectionIds(ids)
				.Authorize(Authorization.AuthorizationFlags.None)
				.CollectAsync();
			await this._deleterFactory.Deleter<DatasetCollectionDeleter>().Delete(datasetCollections);

			this._dbContext.RemoveRange(datas);

			this._eventBroker.EmitCollectionDeleted(datas.Select(x => x.Id).ToList());
		}
	}
}
