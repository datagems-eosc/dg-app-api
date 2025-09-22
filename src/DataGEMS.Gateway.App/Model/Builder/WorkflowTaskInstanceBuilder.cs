using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowTaskInstanceBuilder : Builder<WorkflowTaskInstance, Service.Airflow.Model.AirflowTaskInstance>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowTaskInstanceBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowTaskInstanceBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowTaskInstanceBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override async Task<List<WorkflowTaskInstance>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskInstance> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowTaskInstance)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any()) Enumerable.Empty<WorkflowTaskInstance>().ToList();

			IFieldSet workflowFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowTaskInstance.Workflow)));
			Dictionary<String, WorkflowDefinition> workflowMap = await this.CollectWorkflowDefinitions(workflowFields, datas);

			IFieldSet taskFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowTaskInstance.Task)));
			Dictionary<String, WorkflowTask> workflowTaskMap = await this.CollectWorkflowTasks(taskFields, datas);

			IFieldSet executionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowTaskInstance.WorkflowExecution)));
			Dictionary<String, WorkflowExecution> workflowExecutionMap = await this.CollectWorkflowExecutions(executionFields, datas);

			List<WorkflowTaskInstance> results = new List<WorkflowTaskInstance>();

			foreach (Service.Airflow.Model.AirflowTaskInstance d in datas)
			{
				WorkflowTaskInstance m = new WorkflowTaskInstance();

				if (fields.HasField(nameof(WorkflowTaskInstance.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(WorkflowTaskInstance.WorkflowId))) m.WorkflowId = d.DagId;
				if (fields.HasField(nameof(WorkflowTaskInstance.WorkflowTaskId))) m.WorkflowTaskId = d.TaskId;
				if (fields.HasField(nameof(WorkflowTaskInstance.WorkflowExecutionId))) m.WorkflowExecutionId = d.DagRunId;
				if (fields.HasField(nameof(WorkflowTaskInstance.MapIndex))) m.MapIndex = d.MapIndex;
				if (fields.HasField(nameof(WorkflowTaskInstance.LogicalDate))) m.LogicalDate = d.LogicalDate;
				if (fields.HasField(nameof(WorkflowTaskInstance.RunAfter))) m.RunAfter = d.RunAfter;
				if (fields.HasField(nameof(WorkflowTaskInstance.Start))) m.Start = d.Start;
				if (fields.HasField(nameof(WorkflowTaskInstance.End))) m.End = d.End;
				if (fields.HasField(nameof(WorkflowTaskInstance.Duration))) m.Duration = d.Duration;
				if (fields.HasField(nameof(WorkflowTaskInstance.State)) && Enum.TryParse<Common.WorkflowTaskInstanceState>(d.State, true, out Common.WorkflowTaskInstanceState state)) m.State = state;
				if (fields.HasField(nameof(WorkflowTaskInstance.TryNumber))) m.TryNumber = d.TryNumber;
				if (fields.HasField(nameof(WorkflowTaskInstance.MaxTries))) m.MaxTries = d.MaxTries;
				if (fields.HasField(nameof(WorkflowTaskInstance.TaskDisplayName))) m.TaskDisplayName = d.TaskDisplayName;
				if (fields.HasField(nameof(WorkflowTaskInstance.Hostname))) m.Hostname = d.Hostname;
				if (fields.HasField(nameof(WorkflowTaskInstance.Unixname))) m.Unixname = d.Unixname;
				if (fields.HasField(nameof(WorkflowTaskInstance.Pool))) m.Pool = d.Pool;
				if (fields.HasField(nameof(WorkflowTaskInstance.PoolSlots))) m.PoolSlots = d.PoolSlots;
				if (fields.HasField(nameof(WorkflowTaskInstance.Queue))) m.Queue = d.Queue;
				if (fields.HasField(nameof(WorkflowTaskInstance.QueuedWhen))) m.QueuedWhen = d.QueuedWhen;
				if (fields.HasField(nameof(WorkflowTaskInstance.ScheduledWhen))) m.ScheduledWhen = d.ScheduledWhen;
				if (fields.HasField(nameof(WorkflowTaskInstance.PriorityWeight))) m.PriorityWeight = d.PriorityWeight;
				if (fields.HasField(nameof(WorkflowTaskInstance.Operator))) m.Operator = d.Operator;
				if (fields.HasField(nameof(WorkflowTaskInstance.Pid))) m.Pid = d.Pid;
				if (fields.HasField(nameof(WorkflowTaskInstance.Executor))) m.Executor = d.Executor;
				if (fields.HasField(nameof(WorkflowTaskInstance.ExecutorConfig))) m.ExecutorConfig = d.ExecutorConfig;
				if (fields.HasField(nameof(WorkflowTaskInstance.Note))) m.Note = d.Note;
				if (fields.HasField(nameof(WorkflowTaskInstance.RenderedMapIndex))) m.RenderedMapIndex = d.RenderedMapIndex;
				if (fields.HasField(nameof(WorkflowTaskInstance.RenderedFields))) m.RenderedFields = d.RenderedFields;
				if (fields.HasField(nameof(WorkflowTaskInstance.Trigger))) m.Trigger = d.Trigger;
				if (fields.HasField(nameof(WorkflowTaskInstance.TriggererJob))) m.TriggererJob = d.TriggererJob;
				if (fields.HasField(nameof(WorkflowTaskInstance.DagVersion))) m.DagVersion = d.DagVersion;
				if (!workflowFields.IsEmpty() && workflowMap != null && workflowMap.ContainsKey(d.DagId)) m.Workflow = workflowMap[d.DagId];
				if (!taskFields.IsEmpty() && workflowTaskMap != null && workflowTaskMap.ContainsKey(d.TaskId)) m.Task = workflowTaskMap[d.TaskId];
				if (!executionFields.IsEmpty() && workflowExecutionMap != null && workflowExecutionMap.ContainsKey(d.DagRunId)) m.WorkflowExecution = workflowExecutionMap[d.DagRunId];

				results.Add(m);
			}

			return results;
		}

		private async Task<Dictionary<String, WorkflowDefinition>> CollectWorkflowDefinitions(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskInstance> datas)
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

		private async Task<Dictionary<String, WorkflowTask>> CollectWorkflowTasks(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskInstance> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.WorkflowTask)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<String, WorkflowTask> itemMap = new Dictionary<String, WorkflowTask>();

			var tupleIds = datas.Select(x => new { WorkflowId = x.DagId, TaskId = x.TaskId }).Distinct().ToList();
			foreach (var tupleId in tupleIds)
			{
				Service.Airflow.Model.AirflowTask workflowTaskData = await this._queryFactory.Query<WorkflowTaskHttpQuery>().WorkflowId(tupleId.WorkflowId).TaskId(tupleId.TaskId).ByIdAsync();
				Model.WorkflowTask workflowTaskModel = await this._builderFactory.Builder<WorkflowTaskBuilder>().Authorize(this._authorize).Build(fields, workflowTaskData);
				if (workflowTaskModel != null) itemMap[tupleId.TaskId] = workflowTaskModel;
			}

			return itemMap;
		}

		private async Task<Dictionary<String, WorkflowExecution>> CollectWorkflowExecutions(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskInstance> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.WorkflowExecution)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<String, WorkflowExecution> itemMap = new Dictionary<String, WorkflowExecution>();

			var tupleIds = datas.Select(x => new { WorkflowId = x.DagId, ExecutionId = x.DagRunId }).Distinct().ToList();
			foreach (var tupleId in tupleIds)
			{
				Service.Airflow.Model.AirflowDagExecution workflowExecutionData = await this._queryFactory.Query<WorkflowExecutionHttpQuery>().WorkflowIds(tupleId.WorkflowId).Id(tupleId.ExecutionId).ByIdAsync();
				Model.WorkflowExecution workflowExecutionModel = await this._builderFactory.Builder<WorkflowExecutionBuilder>().Authorize(this._authorize).Build(fields, workflowExecutionData);
				if (workflowExecutionModel != null) itemMap[tupleId.ExecutionId] = workflowExecutionModel;
			}

			return itemMap;
		}
	}
}
