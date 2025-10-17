using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Service.DataManagement;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
    public class CollectionLocalQuery : Cite.Tools.Data.Query.Query<App.Service.DataManagement.Data.Collection>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private String _like { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public CollectionLocalQuery(
		    App.Service.DataManagement.Data.DataManagementDbContext dbContext,
			QueryFactory queryFactory,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._queryFactory = queryFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly App.Service.DataManagement.Data.DataManagementDbContext _dbContext;
		private readonly QueryFactory _queryFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public CollectionLocalQuery Ids(IEnumerable<Guid> ids) { this._ids = ids?.ToList(); return this; }
		public CollectionLocalQuery Ids(Guid id) { this._ids = id.AsList(); return this; }
		public CollectionLocalQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public CollectionLocalQuery ExcludedIds(Guid excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public CollectionLocalQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = datasetIds?.ToList(); return this; }
		public CollectionLocalQuery DatasetIds(Guid datasetId) { this._datasetIds = datasetId.AsList(); return this; }
		public CollectionLocalQuery Like(String like) { this._like = like; return this; }
		public CollectionLocalQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }
		public CollectionLocalQuery EnableTracking() { base.NoTracking = false; return this; }
		public CollectionLocalQuery DisableTracking() { base.NoTracking = true; return this; }
		public CollectionLocalQuery AsDistinct() { base.Distinct = true; return this; }
		public CollectionLocalQuery AsNotDistinct() { base.Distinct = false; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._datasetIds);
		}

		public async Task<App.Service.DataManagement.Data.Collection> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.Collections.FindAsync(id);
			else return await this._dbContext.Collections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<App.Service.DataManagement.Data.Collection> Queryable()
		{
			IQueryable<App.Service.DataManagement.Data.Collection> query = this._dbContext.Collections.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<App.Service.DataManagement.Data.Collection>> ApplyAuthzAsync(IQueryable<App.Service.DataManagement.Data.Collection> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission) && await this._authorizationContentResolver.HasPermission(Permission.BrowseCollection)) return query;
			List<IQueryable<Guid>> predicates = new List<IQueryable<Guid>>();
			if (this._authorize.HasFlag(AuthorizationFlags.Context))
			{
				HashSet<Guid> permittedCollectionIds = new HashSet<Guid>();

				List<String> affiliatedDatasetGroups = await this._authorizationContentResolver.AffiliatedDatasetGroupCodes();
				if (affiliatedDatasetGroups.Count > 0)
				{
					List<Guid> groupIds = await this._dbContext.Collections.Where(x => affiliatedDatasetGroups.Contains(x.Code)).Select(x => x.Id).Distinct().ToListAsync();
					if (groupIds != null) permittedCollectionIds.AddRange(groupIds);
				}

				if (permittedCollectionIds != null && permittedCollectionIds.Count > 0)
				{
					predicates.Add(System.Linq.Queryable.Where(this._dbContext.Collections, x =>
												permittedCollectionIds.Contains(x.Id))
											.Select(x => x.Id));
				}
			}
			//AuthorizationFlags.Owner not applicable
			if (predicates.Count == 0) return query.Where(x => false);

			IQueryable<Guid> union = this.AsUnion(predicates);
			return query.Where(x => union.Contains(x.Id));
		}

		protected override Task<IQueryable<App.Service.DataManagement.Data.Collection>> ApplyFiltersAsync(IQueryable<App.Service.DataManagement.Data.Collection> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._datasetIds != null) query = query.Where(x => x.Datasets.Any(y => this._datasetIds.Contains(y.DatasetId)));
			if (!String.IsNullOrEmpty(this._like)) query = query.Where(x => EF.Functions.ILike(x.Name, this._like));

			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<App.Service.DataManagement.Data.Collection> OrderClause(IQueryable<App.Service.DataManagement.Data.Collection> query, Cite.Tools.Data.Query.OrderingFieldResolver item)
		{
			IOrderedQueryable<App.Service.DataManagement.Data.Collection> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<App.Service.DataManagement.Data.Collection>;

			if (item.Match(nameof(Service.DataManagement.Model.Collection.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Service.DataManagement.Model.Collection.Code))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Code);
			else if (item.Match(nameof(Service.DataManagement.Model.Collection.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Name);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<Cite.Tools.Data.Query.FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (Cite.Tools.Data.Query.FieldResolver item in items)
			{
				if (item.Match(nameof(Service.DataManagement.Model.Collection.Id))) projectionFields.Add(nameof(App.Service.DataManagement.Data.Collection.Id));
				else if (item.Match(nameof(Service.DataManagement.Model.Collection.Code))) projectionFields.Add(nameof(App.Service.DataManagement.Data.Collection.Code));
				else if (item.Match(nameof(Service.DataManagement.Model.Collection.Name))) projectionFields.Add(nameof(App.Service.DataManagement.Data.Collection.Name));
				else if (item.Match(nameof(Service.DataManagement.Model.Collection.DatasetCount))) projectionFields.Add(nameof(App.Service.DataManagement.Data.Collection.Id));
			}
			return projectionFields.ToList();
		}

		public async Task<List<Service.DataManagement.Model.Collection>> CollectAsyncAsModels()
		{
			List<App.Service.DataManagement.Data.Collection> datas = await this.CollectAsync();
			List<Service.DataManagement.Model.Collection> models = datas.Select(x => x.ToModel()).ToList();

			List<Guid> ids = datas.Select(x => x.Id).Distinct().ToList();
			if (ids.Count > 0)
			{
				IQueryable<App.Service.DataManagement.Data.DatasetCollection> query = await this._queryFactory.Query<DatasetCollectionLocalQuery>().Authorize(this._authorize).CollectionIds(ids).ApplyAsync();
				var byCollectionCount = await query.GroupBy(x => x.CollectionId).Select(x => new { x.Key, Count = x.Count() }).ToListAsync();
				Dictionary<Guid, int> byCollectionCountMap = byCollectionCount.ToDictionary(x => x.Key, x => x.Count);
				foreach(Service.DataManagement.Model.Collection model in models)
				{
					if(byCollectionCountMap.ContainsKey(model.Id)) model.DatasetCount = byCollectionCountMap[model.Id];
				}
			}

			return models;
		}

		public async Task<Service.DataManagement.Model.Collection> FirstAsyncAsModel()
		{
			App.Service.DataManagement.Data.Collection datas = await this.FirstAsync();
			Service.DataManagement.Model.Collection models = datas.ToModel();
			return models;
		}
	}
}
