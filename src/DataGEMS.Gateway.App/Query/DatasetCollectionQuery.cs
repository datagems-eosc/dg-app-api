using Cite.Tools.Common.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
    public class DatasetCollectionQuery : Cite.Tools.Data.Query.Query<Data.DatasetCollection>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _collectionIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public DatasetCollectionQuery(
			App.Data.AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly App.Data.AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public DatasetCollectionQuery Ids(IEnumerable<Guid> ids) { this._ids = ids?.ToList(); return this; }
		public DatasetCollectionQuery Ids(Guid id) { this._ids = id.AsList(); return this; }
		public DatasetCollectionQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public DatasetCollectionQuery ExcludedIds(Guid excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public DatasetCollectionQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = datasetIds?.ToList(); return this; }
		public DatasetCollectionQuery DatasetIds(Guid datasetId) { this._datasetIds = datasetId.AsList(); return this; }
		public DatasetCollectionQuery CollectionIds(IEnumerable<Guid> collectionIds) { this._collectionIds = collectionIds?.ToList(); return this; }
		public DatasetCollectionQuery CollectionIds(Guid collectionId) { this._collectionIds = collectionId.AsList(); return this; }
		public DatasetCollectionQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }
		public DatasetCollectionQuery EnableTracking() { base.NoTracking = false; return this; }
		public DatasetCollectionQuery DisableTracking() { base.NoTracking = true; return this; }
		public DatasetCollectionQuery AsDistinct() { base.Distinct = true; return this; }
		public DatasetCollectionQuery AsNotDistinct() { base.Distinct = false; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._datasetIds) || this.IsEmpty(this._collectionIds);
		}

		public async Task<DatasetCollection> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.DatasetCollections.FindAsync(id);
			else return await this._dbContext.DatasetCollections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<DatasetCollection> Queryable()
		{
			IQueryable<DatasetCollection> query = this._dbContext.DatasetCollections.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<DatasetCollection>> ApplyAuthzAsync(IQueryable<DatasetCollection> query)
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

		protected override Task<IQueryable<DatasetCollection>> ApplyFiltersAsync(IQueryable<DatasetCollection> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._collectionIds != null) query = query.Where(x => this._collectionIds.Contains(x.CollectionId));
			if (this._datasetIds != null) query = query.Where(x => this._datasetIds.Contains(x.DatasetId));

			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<DatasetCollection> OrderClause(IQueryable<DatasetCollection> query, Cite.Tools.Data.Query.OrderingFieldResolver item)
		{
			IOrderedQueryable<DatasetCollection> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<DatasetCollection>;

			if (item.Match(nameof(Model.DatasetCollection.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.DatasetCollection.DatasetId))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.DatasetId);
			else if (item.Match(nameof(Model.DatasetCollection.CollectionId))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CollectionId);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<Cite.Tools.Data.Query.FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (Cite.Tools.Data.Query.FieldResolver item in items)
			{
				if (item.Match(nameof(Model.DatasetCollection.Id))) projectionFields.Add(nameof(DatasetCollection.Id));
				else if (item.Match(nameof(Model.DatasetCollection.CollectionId))) projectionFields.Add(nameof(DatasetCollection.CollectionId));
				else if (item.Match(nameof(Model.DatasetCollection.DatasetId))) projectionFields.Add(nameof(DatasetCollection.DatasetId));
			}
			return projectionFields.ToList();
		}
	}
}
