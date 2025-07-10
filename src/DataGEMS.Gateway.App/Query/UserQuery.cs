using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class UserQuery : Query<User>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<String> _idpSubjectIds { get; set; }
		private String _like { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public UserQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public UserQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public UserQuery IdpSubjectIds(IEnumerable<String> idpSubjectIds) { this._idpSubjectIds = this.ToList(idpSubjectIds); return this; }
		public UserQuery IdpSubjectIds(String idpSubjectId) { this._idpSubjectIds = this.ToList(idpSubjectId.AsArray()); return this; }
		public UserQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public UserQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public UserQuery Like(String like) { this._like = like; return this; }
		public UserQuery EnableTracking() { base.NoTracking = false; return this; }
		public UserQuery DisableTracking() { base.NoTracking = true; return this; }
		public UserQuery AsDistinct() { base.Distinct = true; return this; }
		public UserQuery AsNotDistinct() { base.Distinct = false; return this; }
		public UserQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._idpSubjectIds) || this.IsEmpty(this._excludedIds);
		}

		public async Task<User> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.Users.FindAsync(id);
			else return await this._dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<User> Queryable()
		{
			IQueryable<User> query = this._dbContext.Users.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<User>> ApplyAuthzAsync(IQueryable<User> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseUser)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = this._authorizationContentResolver.CurrentUser();
				if (!String.IsNullOrEmpty(currentUser)) return query.Where(x => x.IdpSubjectId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override Task<IQueryable<User>> ApplyFiltersAsync(IQueryable<User> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._idpSubjectIds != null) query = query.Where(x => this._idpSubjectIds.Contains(x.IdpSubjectId));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (!String.IsNullOrEmpty(this._like)) query = query.Where(x => EF.Functions.ILike(x.Name, this._like) || EF.Functions.ILike(x.Email, this._like));
			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<User> OrderClause(IQueryable<User> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<User> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<User>;

			if (item.Match(nameof(Model.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Name);
			else if (item.Match(nameof(Model.User.Email))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Email);
			else if (item.Match(nameof(Model.User.IdpSubjectId))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.IdpSubjectId);
			else if (item.Match(nameof(Model.User.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.User.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.User.Id))) projectionFields.Add(nameof(User.Id));
				else if (item.Match(nameof(Model.User.Name))) projectionFields.Add(nameof(User.Name));
				else if (item.Match(nameof(Model.User.Email))) projectionFields.Add(nameof(User.Email));
				else if (item.Match(nameof(Model.User.IdpSubjectId))) projectionFields.Add(nameof(User.IdpSubjectId));
				else if (item.Match(nameof(Model.User.CreatedAt))) projectionFields.Add(nameof(User.CreatedAt));
				else if (item.Match(nameof(Model.User.UpdatedAt))) projectionFields.Add(nameof(User.UpdatedAt));
				else if (item.Match(nameof(Model.User.ETag))) projectionFields.Add(nameof(User.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
