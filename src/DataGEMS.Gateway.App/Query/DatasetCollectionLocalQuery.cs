using Cite.Tools.Common.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
    public class DatasetCollectionLocalQuery : Cite.Tools.Data.Query.Query<Service.DataManagement.Data.DatasetCollection>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _collectionIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public DatasetCollectionLocalQuery(
			Service.DataManagement.Data.DataManagementDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly Service.DataManagement.Data.DataManagementDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public DatasetCollectionLocalQuery Ids(IEnumerable<Guid> ids) { this._ids = ids?.ToList(); return this; }
		public DatasetCollectionLocalQuery Ids(Guid id) { this._ids = id.AsList(); return this; }
		public DatasetCollectionLocalQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public DatasetCollectionLocalQuery ExcludedIds(Guid excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public DatasetCollectionLocalQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = datasetIds?.ToList(); return this; }
		public DatasetCollectionLocalQuery DatasetIds(Guid datasetId) { this._datasetIds = datasetId.AsList(); return this; }
		public DatasetCollectionLocalQuery CollectionIds(IEnumerable<Guid> collectionIds) { this._collectionIds = collectionIds?.ToList(); return this; }
		public DatasetCollectionLocalQuery CollectionIds(Guid collectionId) { this._collectionIds = collectionId.AsList(); return this; }
		public DatasetCollectionLocalQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }
		public DatasetCollectionLocalQuery EnableTracking() { base.NoTracking = false; return this; }
		public DatasetCollectionLocalQuery DisableTracking() { base.NoTracking = true; return this; }
		public DatasetCollectionLocalQuery AsDistinct() { base.Distinct = true; return this; }
		public DatasetCollectionLocalQuery AsNotDistinct() { base.Distinct = false; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._datasetIds) || this.IsEmpty(this._collectionIds);
		}

		public async Task<Service.DataManagement.Data.DatasetCollection> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.DatasetCollections.FindAsync(id);
			else return await this._dbContext.DatasetCollections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<Service.DataManagement.Data.DatasetCollection> Queryable()
		{
			IQueryable<Service.DataManagement.Data.DatasetCollection> query = this._dbContext.DatasetCollections.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<Service.DataManagement.Data.DatasetCollection>> ApplyAuthzAsync(IQueryable<Service.DataManagement.Data.DatasetCollection> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission) && await this._authorizationContentResolver.HasPermission(Permission.BrowseDatasetCollection)) return query;
			List<IQueryable<Guid>> predicates = new List<IQueryable<Guid>>();
			if (this._authorize.HasFlag(AuthorizationFlags.Context))
			{
				List<Guid> permittedGroupIds = await this._authorizationContentResolver.ContextAffiliatedCollections(Permission.BrowseDatasetCollection);
				if (permittedGroupIds != null && permittedGroupIds.Count > 0)
				{
					predicates.Add(System.Linq.Queryable.Where(this._dbContext.DatasetCollections, x =>
												permittedGroupIds.Contains(x.CollectionId))
											.Select(x => x.Id));
				}
			}
			//AuthorizationFlags.Owner not applicable
			if (predicates.Count == 0) return query.Where(x => false);

			IQueryable<Guid> union = this.AsUnion(predicates);
			return query.Where(x => union.Contains(x.Id));
		}

		protected override Task<IQueryable<Service.DataManagement.Data.DatasetCollection>> ApplyFiltersAsync(IQueryable<Service.DataManagement.Data.DatasetCollection> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._collectionIds != null) query = query.Where(x => this._collectionIds.Contains(x.CollectionId));
			if (this._datasetIds != null) query = query.Where(x => this._datasetIds.Contains(x.DatasetId));

			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<Service.DataManagement.Data.DatasetCollection> OrderClause(IQueryable<Service.DataManagement.Data.DatasetCollection> query, Cite.Tools.Data.Query.OrderingFieldResolver item)
		{
			IOrderedQueryable<Service.DataManagement.Data.DatasetCollection> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<Service.DataManagement.Data.DatasetCollection>;

			if (item.Match(nameof(Service.DataManagement.Model.DatasetCollection.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Service.DataManagement.Model.DatasetCollection.DatasetId))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.DatasetId);
			else if (item.Match(nameof(Service.DataManagement.Model.DatasetCollection.CollectionId))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CollectionId);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<Cite.Tools.Data.Query.FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (Cite.Tools.Data.Query.FieldResolver item in items)
			{
				if (item.Match(nameof(Service.DataManagement.Model.DatasetCollection.Id))) projectionFields.Add(nameof(Service.DataManagement.Data.DatasetCollection.Id));
				else if (item.Match(nameof(Service.DataManagement.Model.DatasetCollection.CollectionId))) projectionFields.Add(nameof(Service.DataManagement.Data.DatasetCollection.CollectionId));
				else if (item.Match(nameof(Service.DataManagement.Model.DatasetCollection.DatasetId))) projectionFields.Add(nameof(Service.DataManagement.Data.DatasetCollection.DatasetId));
			}
			return projectionFields.ToList();
		}

		public async Task<List<Service.DataManagement.Model.DatasetCollection>> CollectAsyncAsModels()
		{
			List<Service.DataManagement.Data.DatasetCollection> datas = await this.CollectAsync();
			List<Service.DataManagement.Model.DatasetCollection> models = datas.Select(x => DatasetCollectionLocalQuery.ToModel(x)).ToList();
			return models;
		}

		public async Task<Service.DataManagement.Model.DatasetCollection> FirstAsyncAsModel()
		{
			Service.DataManagement.Data.DatasetCollection datas = await this.FirstAsync();
			Service.DataManagement.Model.DatasetCollection models = DatasetCollectionLocalQuery.ToModel(datas);
			return models;
		}

		private static Service.DataManagement.Model.DatasetCollection ToModel(Service.DataManagement.Data.DatasetCollection data)
		{
			if (data == null) return null;
			return new Service.DataManagement.Model.DatasetCollection()
			{
				Id = data.Id,
				DatasetId = data.DatasetId,
				CollectionId = data.CollectionId,
			};
		}
	}
}
