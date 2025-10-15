using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class UserSettingsQuery : Query<UserSettings>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<String> _keys { get; set; }
		private List<Guid> _userIds { get; set; }
		private String _like { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserSettingsQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public UserSettingsQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public UserSettingsQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public UserSettingsQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public UserSettingsQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public UserSettingsQuery Keys(IEnumerable<string> keys) { this._keys = this.ToList(keys); return this; }
		public UserSettingsQuery Keys(string key) { this._keys = this.ToList(key.AsArray()); return this; }
		public UserSettingsQuery UserIds(IEnumerable<Guid> userIds) { this._userIds = this.ToList(userIds); return this; }
		public UserSettingsQuery UserIds(Guid userId) { this._userIds = this.ToList(userId.AsArray()); return this; }
		public UserSettingsQuery Like(String like) { this._like = like; return this; }
		public UserSettingsQuery EnableTracking() { base.NoTracking = false; return this; }
		public UserSettingsQuery DisableTracking() { base.NoTracking = true; return this; }
		public UserSettingsQuery AsDistinct() { base.Distinct = true; return this; }
		public UserSettingsQuery AsNotDistinct() { base.Distinct = false; return this; }
		public UserSettingsQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._keys) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._userIds);
		}

		public async Task<UserSettings> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.UserSettings.FindAsync(id);
			else return await this._dbContext.UserSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<UserSettings> Queryable()
		{
			IQueryable<UserSettings> query = this._dbContext.UserSettings.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<UserSettings>> ApplyAuthzAsync(IQueryable<UserSettings> query)
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

		protected override Task<IQueryable<UserSettings>> ApplyFiltersAsync(IQueryable<UserSettings> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._keys != null) query = query.Where(x => this._keys.Contains(x.Key));
			if (this._userIds != null) query = query.Where(x => this._userIds.Contains(x.UserId));
			if (!String.IsNullOrEmpty(this._like)) query = query.Where(x => EF.Functions.ILike(x.Key, this._like));
			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<UserSettings> OrderClause(IQueryable<UserSettings> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<UserSettings> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<UserSettings>;

			if (item.Match(nameof(Model.UserSettings.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.UserSettings.Key))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Key);
			else if (item.Match(nameof(Model.UserSettings.User), nameof(Model.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserId);
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
				if (item.Match(nameof(Model.UserSettings.Id))) projectionFields.Add(nameof(UserSettings.Id));
				else if (item.Match(nameof(Model.UserSettings.Key))) projectionFields.Add(nameof(UserSettings.Key));
				else if (item.Prefix(nameof(Model.UserSettings.User))) projectionFields.Add(nameof(UserSettings.UserId));
				else if (item.Match(nameof(Model.UserSettings.Value))) projectionFields.Add(nameof(UserSettings.Value));
				else if (item.Match(nameof(Model.UserSettings.CreatedAt))) projectionFields.Add(nameof(User.CreatedAt));
				else if (item.Match(nameof(Model.UserSettings.UpdatedAt))) projectionFields.Add(nameof(User.UpdatedAt));
				else if (item.Match(nameof(Model.UserSettings.ETag))) projectionFields.Add(nameof(User.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
