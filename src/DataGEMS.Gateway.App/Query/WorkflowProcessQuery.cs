using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Data;

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowProcessQuery : Query<WorkflowProcess>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid?> _userIds { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowProcessQuery(AppDbContext dbContext, IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public WorkflowProcessQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public WorkflowProcessQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public WorkflowProcessQuery UserIds(IEnumerable<Guid?> userIds) { this._userIds = this.ToList(userIds); return this; }
		public WorkflowProcessQuery UserIds(Guid? userId) { this._userIds = this.ToList(userId.AsArray()); return this; }
		public WorkflowProcessQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public WorkflowProcessQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public WorkflowProcessQuery EnableTracking() { base.NoTracking = false; return this; }
		public WorkflowProcessQuery DisableTracking() { base.NoTracking = true; return this; }
		public WorkflowProcessQuery AsDistinct() { base.Distinct = true; return this; }
		public WorkflowProcessQuery AsNotDistinct() { base.Distinct = false; return this; }
		public WorkflowProcessQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._userIds) || this.IsEmpty(this._excludedIds);
		}

		protected override IQueryable<WorkflowProcess> Queryable()
		{
			IQueryable<WorkflowProcess> query = this._dbContext.WorkflowProcesses.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<WorkflowProcess>> ApplyAuthzAsync(IQueryable<WorkflowProcess> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseWorkflowProcess)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				Guid? currentUser = await this._authorizationContentResolver.CurrentUserId();
				if (currentUser != null) return query.Where(x => x.UserId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override Task<IQueryable<WorkflowProcess>> ApplyFiltersAsync(IQueryable<WorkflowProcess> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._userIds != null) query = query.Where(x => this._userIds.Contains(x.UserId));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<WorkflowProcess> OrderClause(IQueryable<WorkflowProcess> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<WorkflowProcess> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<WorkflowProcess>;

			if (item.Match(nameof(Model.WorkflowProcess.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.WorkflowProcess.ProcessId))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.ProcessId);
			else if (item.Match(nameof(Model.WorkflowProcess.User))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserId);
			else if (item.Match(nameof(Model.WorkflowProcess.Status))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Status);
			else if (item.Match(nameof(Model.WorkflowProcess.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.WorkflowProcess.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<string> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<string> projectionFields = [];
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.WorkflowProcess.Id))) projectionFields.Add(nameof(WorkflowProcess.Id));
				else if (item.Match(nameof(Model.WorkflowProcess.ProcessId))) projectionFields.Add(nameof(WorkflowProcess.ProcessId));
				else if (item.Match(nameof(Model.WorkflowProcess.User))) projectionFields.Add(nameof(WorkflowProcess.UserId));
				else if (item.Match(nameof(Model.WorkflowProcess.CreatedAt))) projectionFields.Add(nameof(WorkflowProcess.CreatedAt));
				else if (item.Match(nameof(Model.WorkflowProcess.UpdatedAt))) projectionFields.Add(nameof(WorkflowProcess.UpdatedAt));
				else if (item.Match(nameof(Model.WorkflowProcess.Status))) projectionFields.Add(nameof(WorkflowProcess.Status));
			}
			return projectionFields.ToList();
		}
	}
}
