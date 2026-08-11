using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Data;

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowProcessStepQuery : Query<WorkflowProcessStep>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _processIds { get; set; }
		private List<Guid> _stepIds { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowProcessStepQuery(AppDbContext dbContext, IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public WorkflowProcessStepQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public WorkflowProcessStepQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public WorkflowProcessStepQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public WorkflowProcessStepQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public WorkflowProcessStepQuery ProcessIds(IEnumerable<Guid> processIds) { this._processIds = this.ToList(processIds); return this; }
		public WorkflowProcessStepQuery ProcessIds(Guid processId) { this._processIds = this.ToList(processId.AsArray()); return this; }
		public WorkflowProcessStepQuery StepIds(IEnumerable<Guid> stepIds) { this._stepIds = this.ToList(stepIds); return this; }
		public WorkflowProcessStepQuery StepIds(Guid stepId) { this._stepIds = this.ToList(stepId.AsArray()); return this; }
		public WorkflowProcessStepQuery EnableTracking() { base.NoTracking = false; return this; }
		public WorkflowProcessStepQuery DisableTracking() { base.NoTracking = true; return this; }
		public WorkflowProcessStepQuery AsDistinct() { base.Distinct = true; return this; }
		public WorkflowProcessStepQuery AsNotDistinct() { base.Distinct = false; return this; }
		public WorkflowProcessStepQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._processIds) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._stepIds);
		}

		protected override IQueryable<WorkflowProcessStep> Queryable()
		{
			IQueryable<WorkflowProcessStep> query = this._dbContext.WorkflowProcessSteps.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<WorkflowProcessStep>> ApplyAuthzAsync(IQueryable<WorkflowProcessStep> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseWorkflowProcessStep)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				Guid? currentUser = await this._authorizationContentResolver.CurrentUserId();
				if (currentUser != null) return query.Where(x => x.Process.UserId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override Task<IQueryable<WorkflowProcessStep>> ApplyFiltersAsync(IQueryable<WorkflowProcessStep> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._processIds != null) query = query.Where(x => this._processIds.Contains(x.ProcessId));
			if (this._stepIds != null) query = query.Where(x => this._stepIds.Contains(x.StepId));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<WorkflowProcessStep> OrderClause(IQueryable<WorkflowProcessStep> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<WorkflowProcessStep> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<WorkflowProcessStep>;

			if (item.Match(nameof(Model.WorkflowProcessStep.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.WorkflowProcessStep.Process))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.ProcessId);
			else if (item.Match(nameof(Model.WorkflowProcessStep.StepId))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.StepId);
			else if (item.Match(nameof(Model.WorkflowProcessStep.Status))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Status);
			else if (item.Match(nameof(Model.WorkflowProcessStep.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.WorkflowProcessStep.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<string> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<string> projectionFields = [];
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.WorkflowProcessStep.Id))) projectionFields.Add(nameof(WorkflowProcessStep.Id));
				else if (item.Prefix(nameof(Model.WorkflowProcessStep.Process))) projectionFields.Add(nameof(WorkflowProcessStep.ProcessId));
				else if (item.Match(nameof(Model.WorkflowProcessStep.WorkflowTaskInstanceDetails))) projectionFields.Add(nameof(WorkflowProcessStep.WorkflowTaskInstanceDetails));
				else if (item.Match(nameof(Model.WorkflowProcessStep.StepId))) projectionFields.Add(nameof(WorkflowProcessStep.StepId));
				else if (item.Match(nameof(Model.WorkflowProcessStep.CreatedAt))) projectionFields.Add(nameof(WorkflowProcessStep.CreatedAt));
				else if (item.Match(nameof(Model.WorkflowProcessStep.UpdatedAt))) projectionFields.Add(nameof(WorkflowProcessStep.UpdatedAt));
				else if (item.Match(nameof(Model.WorkflowProcessStep.Status))) projectionFields.Add(nameof(WorkflowProcessStep.Status));
			}
			return projectionFields.ToList();
		}
	}
}
