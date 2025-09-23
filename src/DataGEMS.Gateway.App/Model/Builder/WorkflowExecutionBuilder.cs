using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Query;
using Cite.Tools.Data.Query;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowExecutionBuilder : Builder<WorkflowExecution, Service.Airflow.Model.AirflowDagExecution>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowExecutionBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowExecutionBuilder> logger): base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowExecutionBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override async Task<List<WorkflowExecution>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDagExecution> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowDagExecution)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any()) return Enumerable.Empty<WorkflowExecution>().ToList();

			IFieldSet workflowTaskInstanceFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowExecution.TaskInstances)));
			Dictionary<String, List<WorkflowTaskInstance>> workflowTaskInstanceMap = await this.CollectWorkflowTaskInstances(workflowTaskInstanceFields, datas);

			IFieldSet workflowFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowExecution.Workflow)));
			Dictionary<String, WorkflowDefinition> workflowMap = await this.CollectWorkflowDefinitions(workflowFields, datas);

			List<WorkflowExecution> results = new List<WorkflowExecution>();

			foreach (Service.Airflow.Model.AirflowDagExecution d in datas)
			{
				WorkflowExecution m = new WorkflowExecution();

				if (fields.HasField(nameof(WorkflowExecution.Id))) m.Id=d.RunId;
				if (fields.HasField(nameof(WorkflowExecution.WorkflowId))) m.WorkflowId = d.DagId;
				if (fields.HasField(nameof(WorkflowExecution.LogicalDate))) m.LogicalDate = d.LogicalDate;
				if (fields.HasField(nameof(WorkflowExecution.QueuedAt))) m.QueuedAt = d.QueuedAt;
				if (fields.HasField(nameof(WorkflowExecution.Start))) m.Start = d.Start;
				if (fields.HasField(nameof(WorkflowExecution.End))) m.End = d.End;
				if (fields.HasField(nameof(WorkflowExecution.DataIntervalStart))) m.DataIntervalStart = d.IntervalStart;
				if (fields.HasField(nameof(WorkflowExecution.DataIntervalEnd))) m.DataIntervalEnd = d.IntervalEnd;
				if (fields.HasField(nameof(WorkflowExecution.RunAfter))) m.RunAfter = d.RunAfter;
				if (fields.HasField(nameof(WorkflowExecution.LastSchedulingDecision))) m.LastSchedulingDecision = d.LastSchedulingDecision;
				if (fields.HasField(nameof(WorkflowExecution.RunType)) && Enum.TryParse<Common.WorkflowRunType>(d.RunType, true, out Common.WorkflowRunType runType)) m.RunType = runType;
				if (fields.HasField(nameof(WorkflowExecution.TriggeredBy))) m.TriggeredBy = d.TriggeredBy;
				if (fields.HasField(nameof(WorkflowExecution.State)) && Enum.TryParse<Common.WorkflowRunState>(d.State, true, out Common.WorkflowRunState runState)) m.State = runState;
				if (fields.HasField(nameof(WorkflowExecution.Note))) m.Note = d.Note;
				if (fields.HasField(nameof(WorkflowExecution.BundleVersion))) m.BundleVersion = d.BundleVersion;
				if (!workflowTaskInstanceFields.IsEmpty() && workflowTaskInstanceMap != null && workflowTaskInstanceMap.ContainsKey(d.RunId)) m.TaskInstances = workflowTaskInstanceMap[d.RunId];
				if (!workflowFields.IsEmpty() && workflowMap != null && workflowMap.ContainsKey(d.DagId)) m.Workflow = workflowMap[d.DagId];

				results.Add(m);
			}

			return results;
		}

		private async Task<Dictionary<String, List<WorkflowTaskInstance>>> CollectWorkflowTaskInstances(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDagExecution> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.WorkflowTaskInstance)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<String, List<WorkflowTaskInstance>> itemMap = new Dictionary<String, List<WorkflowTaskInstance>>();

			var tupleIds = datas.Select(x => new { WorkflowId = x.DagId, ExecutionId = x.RunId }).Distinct().ToList();
			foreach (var tupleId in tupleIds)
			{
				List<Service.Airflow.Model.AirflowTaskInstance> taskInstanceDatas = await this._queryFactory.Query<WorkflowTaskInstanceHttpQuery>().WorkflowIds(tupleId.WorkflowId).WorkflowExecutionIds(tupleId.ExecutionId).CollectAsync();
				List<Model.WorkflowTaskInstance> taskInstanceModels = await this._builderFactory.Builder<WorkflowTaskInstanceBuilder>().Authorize(this._authorize).Build(fields, taskInstanceDatas);
				if (taskInstanceModels != null && taskInstanceModels.Count > 0) itemMap[tupleId.ExecutionId] = taskInstanceModels;
			}

			return itemMap;
		}

		private async Task<Dictionary<String, WorkflowDefinition>> CollectWorkflowDefinitions(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDagExecution> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.WorkflowDefinition)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<String, WorkflowDefinition> itemMap = new Dictionary<String, WorkflowDefinition>();

			List<String> workflowIds = datas.Select(x => x.DagId).Distinct().ToList();
			foreach (String workflowId in workflowIds)
			{
				Service.Airflow.Model.AirflowDag workflowDefinitionData = await this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Id(workflowId).ByIdAsync();
				Model.WorkflowDefinition workflowDefinitionModel = await this._builderFactory.Builder<WorkflowDefinitionBuilder>().Authorize(this._authorize).Build(fields, workflowDefinitionData);
				if (workflowDefinitionModel != null) itemMap[workflowId] = workflowDefinitionModel;
			}

			return itemMap;
		}
	}
}
