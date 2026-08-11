using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Query;
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

			IFieldSet processFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowProcessStep.Process)));
			Dictionary<Guid, WorkflowProcess> processMap = await this.CollectWorkflowProcesses(processFields, datas);

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
				if (!processFields.IsEmpty() && processMap != null && processMap.ContainsKey(d.ProcessId)) m.Process = processMap[d.ProcessId];
				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, WorkflowProcess>> CollectWorkflowProcesses(IFieldSet fields, IEnumerable<Data.WorkflowProcessStep> datas)
		{
			if (fields == null || fields.IsEmpty()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.WorkflowProcess)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, WorkflowProcess> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(WorkflowProcess.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.ProcessId).Distinct(), x => new WorkflowProcess() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(WorkflowProcess.Id));
				WorkflowProcessQuery query = this._queryFactory.Query<WorkflowProcessQuery>().DisableTracking().Ids(datas.Select(x => x.ProcessId).Distinct()).Authorize(this._authorize);
				itemMap = await this._builderFactory.Builder<WorkflowProcessBuilder>().Authorize(this._authorize).AsForeignKey(query, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(WorkflowProcess.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}
	}
}
