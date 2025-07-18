using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class UserCollectionQuery : Query<UserCollection>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _userIds { get; set; }
		private String _like { get; set; }
		private List<IsActive> _isActive { get; set; }
		private List<UserCollectionKind> _kind { get; set; }
		private UserDatasetCollectionQuery _userDatasetCollectionQuery { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserCollectionQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public UserCollectionQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public UserCollectionQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public UserCollectionQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public UserCollectionQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public UserCollectionQuery UserIds(IEnumerable<Guid> userIds) { this._userIds = this.ToList(userIds); return this; }
		public UserCollectionQuery UserIds(Guid userId) { this._userIds = this.ToList(userId.AsArray()); return this; }
		public UserCollectionQuery Like(String like) { this._like = like; return this; }
		public UserCollectionQuery IsActive(IEnumerable<IsActive> isActive) { this._isActive = this.ToList(isActive); return this; }
		public UserCollectionQuery IsActive(IsActive isActive) { this._isActive = this.ToList(isActive.AsArray()); return this; }
		public UserCollectionQuery Kind(IEnumerable<UserCollectionKind> kind) { this._kind = this.ToList(kind); return this; }
		public UserCollectionQuery Kind(UserCollectionKind kind) { this._kind = this.ToList(kind.AsArray()); return this; }
		public UserCollectionQuery UserDatasetCollectionSubQuery(UserDatasetCollectionQuery subquery) { this._userDatasetCollectionQuery = subquery; return this; }
		public UserCollectionQuery EnableTracking() { base.NoTracking = false; return this; }
		public UserCollectionQuery DisableTracking() { base.NoTracking = true; return this; }
		public UserCollectionQuery AsDistinct() { base.Distinct = true; return this; }
		public UserCollectionQuery AsNotDistinct() { base.Distinct = false; return this; }
		public UserCollectionQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._userIds) || 
				this.IsEmpty(this._isActive) || this.IsEmpty(this._kind) || this.IsFalseQuery(this._userDatasetCollectionQuery);
		}

		public async Task<UserCollection> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.UserCollections.FindAsync(id);
			else return await this._dbContext.UserCollections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<UserCollection> Queryable()
		{
			IQueryable<UserCollection> query = this._dbContext.UserCollections.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<UserCollection>> ApplyAuthzAsync(IQueryable<UserCollection> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseUserCollection)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = this._authorizationContentResolver.CurrentUser();
				if (!String.IsNullOrEmpty(currentUser)) return query.Where(x => x.User.IdpSubjectId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override async Task<IQueryable<UserCollection>> ApplyFiltersAsync(IQueryable<UserCollection> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._userIds != null) query = query.Where(x => this._userIds.Contains(x.UserId));
			if (this._isActive != null) query = query.Where(x => this._isActive.Contains(x.IsActive));
			if (this._kind != null) query = query.Where(x => this._kind.Contains(x.Kind));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (!String.IsNullOrEmpty(this._like)) query = query.Where(x => EF.Functions.ILike(x.Name, this._like));
			if (this._userDatasetCollectionQuery != null)
			{
				IQueryable<Guid> subQuery = await this.BindSubQueryAsync(this._userDatasetCollectionQuery, this._dbContext.UserDatasetCollections, y => y.UserCollectionId);
				subQuery = subQuery.Distinct();
				query = query.Where(x => subQuery.Contains(x.Id));
			}
			return query;
		}

		protected override IOrderedQueryable<UserCollection> OrderClause(IQueryable<UserCollection> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<UserCollection> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<UserCollection>;

			if (item.Match(nameof(Model.UserCollection.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.UserCollection.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Name);
			else if (item.Match(nameof(Model.UserCollection.IsActive))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.IsActive);
			else if (item.Match(nameof(Model.UserCollection.Kind))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Kind);
			else if (item.Match(nameof(Model.UserCollection.User), nameof(Model.UserCollection.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserId);
			else if (item.Match(nameof(Model.UserCollection.User), nameof(Model.UserCollection.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Name);
			else if (item.Match(nameof(Model.UserCollection.User), nameof(Model.UserCollection.User.Email))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Email);
			else if (item.Match(nameof(Model.UserCollection.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.UserCollection.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.UserCollection.Id))) projectionFields.Add(nameof(UserCollection.Id));
				else if (item.Match(nameof(Model.UserCollection.Name))) projectionFields.Add(nameof(UserCollection.Name));
				else if (item.Prefix(nameof(Model.UserCollection.User))) projectionFields.Add(nameof(UserCollection.UserId));
				else if (item.Prefix(nameof(Model.UserCollection.UserDatasetCollections))) projectionFields.Add(nameof(UserCollection.Id));
				else if (item.Match(nameof(Model.UserCollection.IsActive))) projectionFields.Add(nameof(UserCollection.IsActive));
				else if (item.Match(nameof(Model.UserCollection.Kind))) projectionFields.Add(nameof(UserCollection.Kind));
				else if (item.Match(nameof(Model.UserCollection.CreatedAt))) projectionFields.Add(nameof(UserCollection.CreatedAt));
				else if (item.Match(nameof(Model.UserCollection.UpdatedAt))) projectionFields.Add(nameof(UserCollection.UpdatedAt));
				else if (item.Match(nameof(Model.UserCollection.ETag))) projectionFields.Add(nameof(UserCollection.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
