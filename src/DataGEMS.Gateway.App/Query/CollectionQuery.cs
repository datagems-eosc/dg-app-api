using Cite.Tools.Common.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class CollectionQuery : ExtendedQuery<Data.Collection>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private String _like { get; set; }
		private List<String> _contextRolesInMemory { get; set; }
		private String _contextRoleSubjectIdInMemory { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public CollectionQuery(
			App.Data.AppDbContext dbContext,
			Cite.Tools.Data.Query.QueryFactory queryFactory,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._queryFactory = queryFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly App.Data.AppDbContext _dbContext;
		private readonly Cite.Tools.Data.Query.QueryFactory _queryFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public CollectionQuery Ids(IEnumerable<Guid> ids) { this._ids = ids?.ToList(); return this; }
		public CollectionQuery Ids(Guid id) { this._ids = id.AsList(); return this; }
		public CollectionQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public CollectionQuery ExcludedIds(Guid excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public CollectionQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = datasetIds?.ToList(); return this; }
		public CollectionQuery DatasetIds(Guid datasetId) { this._datasetIds = datasetId.AsList(); return this; }
		public CollectionQuery Like(String like) { this._like = like; return this; }
		public CollectionQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }
		public CollectionQuery EnableTracking() { base.NoTracking = false; return this; }
		public CollectionQuery DisableTracking() { base.NoTracking = true; return this; }
		public CollectionQuery AsDistinct() { base.Distinct = true; return this; }
		public CollectionQuery AsNotDistinct() { base.Distinct = false; return this; }
		public CollectionQuery ContextRolesInMemory(IEnumerable<String> contextRoles) { this._contextRolesInMemory = contextRoles?.ToList(); return this; }
		public CollectionQuery ContextRolesInMemory(String contextRole) { this._contextRolesInMemory = contextRole.AsList(); return this; }
		public CollectionQuery ContextRoleSubjectIdInMemory(String subjectId) { this._contextRoleSubjectIdInMemory = subjectId; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._datasetIds) || this.IsEmpty(this._contextRolesInMemory);
		}

		protected override bool RequiresInMemoryFiltering()
		{
			return this._contextRolesInMemory != null && this._contextRolesInMemory.Count > 0;
		}

		protected override bool RequiresInMemoryOrdering() { return false; }

		protected override string[] ProjectionEnsureInMemoryProcessing()
		{
			HashSet<string> ensure = [];
			if (this._contextRolesInMemory != null) ensure.Add(nameof(Collection.Id));
			return ensure.ToArray();
		}

		public async Task<Collection> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.Collections.FindAsync(id);
			else return await this._dbContext.Collections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<Collection> Queryable()
		{
			IQueryable<Collection> query = this._dbContext.Collections.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<Collection>> ApplyAuthzAsync(IQueryable<Collection> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission) && await this._authorizationContentResolver.HasPermission(Permission.BrowseCollection)) return query;
			List<IQueryable<Guid>> predicates = new List<IQueryable<Guid>>();
			if (this._authorize.HasFlag(AuthorizationFlags.Context))
			{
				List<Guid> permittedCollectionIds = await this._authorizationContentResolver.ContextAffiliatedCollections(Permission.BrowseCollection);
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

		protected override Task<IQueryable<Collection>> ApplyFiltersAsync(IQueryable<Collection> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._datasetIds != null) query = query.Where(x => x.Datasets.Any(y => this._datasetIds.Contains(y.DatasetId)));
			if (!String.IsNullOrEmpty(this._like)) query = query.Where(x => EF.Functions.ILike(x.Name, this._like));

			return Task.FromResult(query);
		}

		protected override async Task<List<Collection>> FilterAsync(List<Collection> items)
		{
			List<Collection> data = items;

			if (this._contextRolesInMemory != null)
			{
				String contextRoleSubjectId = this._contextRoleSubjectIdInMemory;
				if (String.IsNullOrEmpty(this._contextRoleSubjectIdInMemory)) contextRoleSubjectId = await this._authorizationContentResolver.SubjectIdOfCurrentUser();

				if (String.IsNullOrEmpty(contextRoleSubjectId)) data = new List<Collection>();
				else
				{
					HashSet<Guid> collectionIds = (await this._queryFactory.Query<ContextGrantQuery>()
						.Subject(contextRoleSubjectId)
						.Roles(this._contextRolesInMemory)
						.TargetKinds(Common.Auth.ContextGrant.TargetKind.Collection)
						.CollectAsync()).Select(x => x.TargetId).ToHashSet();
					data = data.Where(x => collectionIds.Contains(x.Id)).ToList();
				}
			}
			return data;
		}

		protected override async Task<List<Collection>> OrderAsync(List<Collection> items)
		{
			return await base.OrderAsync(items);
		}

		protected override IOrderedQueryable<Collection> OrderClause(IQueryable<Collection> query, Cite.Tools.Data.Query.OrderingFieldResolver item)
		{
			IOrderedQueryable<Collection> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<Collection>;

			if (item.Match(nameof(Model.Collection.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.Collection.Code))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Code);
			else if (item.Match(nameof(Model.Collection.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Name);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<Cite.Tools.Data.Query.FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (Cite.Tools.Data.Query.FieldResolver item in items)
			{
				if (item.Match(nameof(Model.Collection.Id))) projectionFields.Add(nameof(Collection.Id));
				else if (item.Match(nameof(Model.Collection.Code))) projectionFields.Add(nameof(Collection.Code));
				else if (item.Match(nameof(Model.Collection.Name))) projectionFields.Add(nameof(Collection.Name));
				else if (item.Match(nameof(Model.Collection.DatasetCount))) projectionFields.Add(nameof(Collection.Id));
			}
			return projectionFields.ToList();
		}
	}
}
