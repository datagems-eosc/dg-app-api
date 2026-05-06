using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class UserFavoriteQuery : Query<Data.UserFavorite>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _userIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private List<IsActive> _isActive { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserFavoriteQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public UserFavoriteQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public UserFavoriteQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public UserFavoriteQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public UserFavoriteQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public UserFavoriteQuery UserIds(IEnumerable<Guid> userIds) { this._userIds = this.ToList(userIds); return this; }
		public UserFavoriteQuery UserIds(Guid userId) { this._userIds = this.ToList(userId.AsArray()); return this; }
		public UserFavoriteQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = this.ToList(datasetIds); return this; }
		public UserFavoriteQuery DatasetIds(Guid datasetId) { this._datasetIds = this.ToList(datasetId.AsArray()); return this; }
		public UserFavoriteQuery IsActive(IEnumerable<IsActive> isActive) { this._isActive = this.ToList(isActive); return this; }
		public UserFavoriteQuery IsActive(IsActive isActive) { this._isActive = this.ToList(isActive.AsArray()); return this; }
		public UserFavoriteQuery EnableTracking() { base.NoTracking = false; return this; }
		public UserFavoriteQuery DisableTracking() { base.NoTracking = true; return this; }
		public UserFavoriteQuery AsDistinct() { base.Distinct = true; return this; }
		public UserFavoriteQuery AsNotDistinct() { base.Distinct = false; return this; }
		public UserFavoriteQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._userIds) || this.IsEmpty(this._datasetIds) || this.IsEmpty(this._isActive);
		}

		public async Task<UserFavorite> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.UserFavorites.FindAsync(id);
			else return await this._dbContext.UserFavorites.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<UserFavorite> Queryable()
		{
			IQueryable<UserFavorite> query = this._dbContext.UserFavorites.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<UserFavorite>> ApplyAuthzAsync(IQueryable<UserFavorite> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				Guid? currentUser = await this._authorizationContentResolver.CurrentUserId();
				if (currentUser.HasValue) return query.Where(x => x.UserId == currentUser);
			}
			//AuthorizationFlags.Context, AuthorizationFlags.Permission not applicable
			return query.Where(x => false);
		}

		protected override Task<IQueryable<UserFavorite>> ApplyFiltersAsync(IQueryable<UserFavorite> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._userIds != null) query = query.Where(x => this._userIds.Contains(x.UserId));
			if (this._isActive != null) query = query.Where(x => this._isActive.Contains(x.IsActive));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._datasetIds != null) query = query.Where(x => this._datasetIds.Contains(x.DatasetId));
			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<UserFavorite> OrderClause(IQueryable<UserFavorite> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<UserFavorite> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<UserFavorite>;

			if (item.Match(nameof(Model.UserFavorite.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.UserFavorite.IsActive))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.IsActive);
			else if (item.Match(nameof(Model.UserFavorite.Dataset), nameof(Model.UserFavorite.Dataset.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.DatasetId);
			else if (item.Match(nameof(Model.UserFavorite.User), nameof(Model.UserFavorite.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserId);
			else if (item.Match(nameof(Model.UserFavorite.User), nameof(Model.UserFavorite.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Name);
			else if (item.Match(nameof(Model.UserFavorite.User), nameof(Model.UserFavorite.User.Email))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Email);
			else if (item.Match(nameof(Model.UserFavorite.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.UserFavorite.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = [];
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.UserFavorite.Id))) projectionFields.Add(nameof(UserFavorite.Id));
				else if (item.Prefix(nameof(Model.UserFavorite.User))) projectionFields.Add(nameof(UserFavorite.UserId));
				else if (item.Prefix(nameof(Model.UserFavorite.Dataset))) projectionFields.Add(nameof(UserFavorite.DatasetId));
				else if (item.Match(nameof(Model.UserFavorite.IsActive))) projectionFields.Add(nameof(UserFavorite.IsActive));
				else if (item.Match(nameof(Model.UserFavorite.CreatedAt))) projectionFields.Add(nameof(UserFavorite.CreatedAt));
				else if (item.Match(nameof(Model.UserFavorite.UpdatedAt))) projectionFields.Add(nameof(UserFavorite.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
