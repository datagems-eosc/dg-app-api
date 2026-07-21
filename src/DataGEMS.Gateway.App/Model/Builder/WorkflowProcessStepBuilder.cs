using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowProcessStepBuilder : Builder<WorkflowProcessStep, Data.WorkflowProcessStep>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowProcessStepBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<WorkflowProcessStepBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public WorkflowProcessStepBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<WorkflowProcessStep>> Build(IFieldSet fields, IEnumerable<Data.WorkflowProcessStep> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.WorkflowProcessStep)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<WorkflowProcessStep>().ToList();

			List<WorkflowProcessStep> models = [];
			foreach (Data.WorkflowProcessStep d in datas ?? [])
			{
				WorkflowProcessStep m = new WorkflowProcessStep();
				if (fields.HasField(nameof(WorkflowProcessStep.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(WorkflowProcessStep.StepId))) m.StepId = d.StepId;
				if (fields.HasField(nameof(WorkflowProcessStep.Status))) m.Status = d.Status;
				if (fields.HasField(nameof(WorkflowProcessStep.WorkflowTaskInstanceDetails))) m.WorkflowTaskInstanceDetails = d.WorkflowTaskInstanceDetails;
				if (fields.HasField(nameof(WorkflowProcessStep.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(WorkflowProcessStep.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;

				models.Add(m);
			}
			return models;
		}
	}
}
