using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class UserDatasetCollectionQuery : Query<UserDatasetCollection>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private List<Guid> _userCollectionIds { get; set; }
		private List<IsActive> _isActive { get; set; }
		private UserCollectionQuery _userCollectionQuery { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserDatasetCollectionQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public UserDatasetCollectionQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public UserDatasetCollectionQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public UserDatasetCollectionQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public UserDatasetCollectionQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public UserDatasetCollectionQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = this.ToList(datasetIds); return this; }
		public UserDatasetCollectionQuery DatasetIds(Guid datasetId) { this._datasetIds = this.ToList(datasetId.AsArray()); return this; }
		public UserDatasetCollectionQuery UserCollectionIds(IEnumerable<Guid> userCollectionIds) { this._userCollectionIds = this.ToList(userCollectionIds); return this; }
		public UserDatasetCollectionQuery UserCollectionIds(Guid userCollectionId) { this._userCollectionIds = this.ToList(userCollectionId.AsArray()); return this; }
		public UserDatasetCollectionQuery UserCollectionSubQuery(UserCollectionQuery subquery) { this._userCollectionQuery = subquery; return this; }
		public UserDatasetCollectionQuery IsActive(IEnumerable<IsActive> isActive) { this._isActive = this.ToList(isActive); return this; }
		public UserDatasetCollectionQuery IsActive(IsActive isActive) { this._isActive = this.ToList(isActive.AsArray()); return this; }
		public UserDatasetCollectionQuery EnableTracking() { base.NoTracking = false; return this; }
		public UserDatasetCollectionQuery DisableTracking() { base.NoTracking = true; return this; }
		public UserDatasetCollectionQuery AsDistinct() { base.Distinct = true; return this; }
		public UserDatasetCollectionQuery AsNotDistinct() { base.Distinct = false; return this; }
		public UserDatasetCollectionQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._datasetIds) || this.IsEmpty(this._userCollectionIds) || 
				this.IsEmpty(this._excludedIds) || this.IsEmpty(this._isActive) || this.IsFalseQuery(this._userCollectionQuery);
		}

		public async Task<UserDatasetCollection> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.UserDatasetCollections.FindAsync(id);
			else return await this._dbContext.UserDatasetCollections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<UserDatasetCollection> Queryable()
		{
			IQueryable<UserDatasetCollection> query = this._dbContext.UserDatasetCollections.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<UserDatasetCollection>> ApplyAuthzAsync(IQueryable<UserDatasetCollection> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseUserDatasetCollection)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = this._authorizationContentResolver.CurrentUser();
				if (!String.IsNullOrEmpty(currentUser)) return query.Where(x => x.UserCollection.User.IdpSubjectId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override async Task<IQueryable<UserDatasetCollection>> ApplyFiltersAsync(IQueryable<UserDatasetCollection> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._datasetIds != null) query = query.Where(x => this._datasetIds.Contains(x.DatasetId));
			if (this._userCollectionIds != null) query = query.Where(x => this._userCollectionIds.Contains(x.UserCollectionId));
			if (this._isActive != null) query = query.Where(x => this._isActive.Contains(x.IsActive));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._userCollectionQuery != null)
			{
				IQueryable<Guid> subQuery = await this.BindSubQueryAsync(this._userCollectionQuery, this._dbContext.UserCollections, y => y.Id);
				subQuery = subQuery.Distinct();
				query = query.Where(x => subQuery.Contains(x.UserCollectionId));
			}
			return query;
		}

		protected override IOrderedQueryable<UserDatasetCollection> OrderClause(IQueryable<UserDatasetCollection> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<UserDatasetCollection> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<UserDatasetCollection>;

			if (item.Match(nameof(Model.UserDatasetCollection.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.UserDatasetCollection.Dataset), nameof(Model.Dataset.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.DatasetId);
			else if (item.Match(nameof(Model.UserDatasetCollection.IsActive))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.IsActive);
			else if (item.Match(nameof(Model.UserDatasetCollection.UserCollection), nameof(Model.UserCollection.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserCollection.Id);
			else if (item.Match(nameof(Model.UserDatasetCollection.UserCollection), nameof(Model.UserCollection.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserCollection.Name);
			else if (item.Match(nameof(Model.UserDatasetCollection.UserCollection), nameof(Model.UserCollection.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserCollection.User.Id);
			else if (item.Match(nameof(Model.UserDatasetCollection.UserCollection), nameof(Model.UserCollection.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserCollection.User.Name);
			else if (item.Match(nameof(Model.UserDatasetCollection.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.UserDatasetCollection.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.UserDatasetCollection.Id))) projectionFields.Add(nameof(UserDatasetCollection.Id));
				else if (item.Prefix(nameof(Model.UserDatasetCollection.Dataset))) projectionFields.Add(nameof(UserDatasetCollection.DatasetId));
				else if (item.Match(nameof(Model.UserDatasetCollection.IsActive))) projectionFields.Add(nameof(UserDatasetCollection.IsActive));
				else if (item.Prefix(nameof(Model.UserDatasetCollection.UserCollection))) projectionFields.Add(nameof(UserDatasetCollection.UserCollectionId));
				else if (item.Match(nameof(Model.UserDatasetCollection.CreatedAt))) projectionFields.Add(nameof(UserDatasetCollection.CreatedAt));
				else if (item.Match(nameof(Model.UserDatasetCollection.UpdatedAt))) projectionFields.Add(nameof(UserDatasetCollection.UpdatedAt));
				else if (item.Match(nameof(Model.UserDatasetCollection.ETag))) projectionFields.Add(nameof(UserDatasetCollection.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
