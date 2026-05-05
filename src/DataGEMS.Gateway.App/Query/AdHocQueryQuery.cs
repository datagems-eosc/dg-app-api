using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using DataGEMS.Gateway.App.Model;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class AdHocQueryQuery : Query<Data.AdHocQueryResult>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _userIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private String _like { get; set; }
		private List<IsActive> _isActive { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public AdHocQueryQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public AdHocQueryQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public AdHocQueryQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public AdHocQueryQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public AdHocQueryQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public AdHocQueryQuery UserIds(IEnumerable<Guid> userIds) { this._userIds = this.ToList(userIds); return this; }
		public AdHocQueryQuery UserIds(Guid userId) { this._userIds = this.ToList(userId.AsArray()); return this; }
		public AdHocQueryQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = this.ToList(datasetIds); return this; }
		public AdHocQueryQuery DatasetIds(Guid datasetId) { this._datasetIds = this.ToList(datasetId.AsArray()); return this; }
		public AdHocQueryQuery Like(String like) { this._like = like; return this; }
		public AdHocQueryQuery IsActive(IEnumerable<IsActive> isActive) { this._isActive = this.ToList(isActive); return this; }
		public AdHocQueryQuery IsActive(IsActive isActive) { this._isActive = this.ToList(isActive.AsArray()); return this; }
		public AdHocQueryQuery EnableTracking() { base.NoTracking = false; return this; }
		public AdHocQueryQuery DisableTracking() { base.NoTracking = true; return this; }
		public AdHocQueryQuery AsDistinct() { base.Distinct = true; return this; }
		public AdHocQueryQuery AsNotDistinct() { base.Distinct = false; return this; }
		public AdHocQueryQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._userIds) ||
				this.IsEmpty(this._isActive) || this.IsEmpty(this._datasetIds);
		}

		public async Task<AdHocQueryResult> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.AdHocQueryResults.FindAsync(id);
			else return await this._dbContext.AdHocQueryResults.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<AdHocQueryResult> Queryable()
		{
			IQueryable<AdHocQueryResult> query = this._dbContext.AdHocQueryResults.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<AdHocQueryResult>> ApplyAuthzAsync(IQueryable<AdHocQueryResult> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseAdHocQuery)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = this._authorizationContentResolver.CurrentUser();
				if (!String.IsNullOrEmpty(currentUser)) return query.Where(x => x.User.IdpSubjectId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override async Task<IQueryable<AdHocQueryResult>> ApplyFiltersAsync(IQueryable<AdHocQueryResult> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._userIds != null) query = query.Where(x => this._userIds.Contains(x.UserId));
			if (this._isActive != null) query = query.Where(x => this._isActive.Contains(x.IsActive));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._datasetIds != null) query = query.Where(x => this._datasetIds.Contains(x.DatasetId));
			return query;
		}

		protected override IOrderedQueryable<AdHocQueryResult> OrderClause(IQueryable<AdHocQueryResult> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<AdHocQueryResult> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<AdHocQueryResult>;

			if (item.Match(nameof(Model.AdHocQuery.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.AdHocQuery.IsActive))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.IsActive);
			else if (item.Match(nameof(Model.AdHocQuery.Dataset), nameof(Model.AdHocQuery.Dataset.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.DatasetId);
			else if (item.Match(nameof(Model.AdHocQuery.User), nameof(Model.AdHocQuery.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserId);
			else if (item.Match(nameof(Model.AdHocQuery.User), nameof(Model.AdHocQuery.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Name);
			else if (item.Match(nameof(Model.AdHocQuery.User), nameof(Model.AdHocQuery.User.Email))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Email);
			else if (item.Match(nameof(Model.AdHocQuery.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.AdHocQuery.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.AdHocQuery.Id))) projectionFields.Add(nameof(AdHocQueryResult.Id));
				else if (item.Prefix(nameof(Model.AdHocQuery.AnalyticalPattern))) projectionFields.Add(nameof(AdHocQueryResult.AnalyticalPattern));
				else if (item.Prefix(nameof(Model.AdHocQuery.User))) projectionFields.Add(nameof(AdHocQueryResult.UserId));
				else if (item.Prefix(nameof(Model.AdHocQuery.Dataset))) projectionFields.Add(nameof(AdHocQueryResult.DatasetId));
				else if (item.Match(nameof(Model.AdHocQuery.IsActive))) projectionFields.Add(nameof(AdHocQueryResult.IsActive));
				else if (item.Match(nameof(Model.AdHocQuery.CreatedAt))) projectionFields.Add(nameof(AdHocQueryResult.CreatedAt));
				else if (item.Match(nameof(Model.AdHocQuery.UpdatedAt))) projectionFields.Add(nameof(AdHocQueryResult.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
