using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowTaskInstanceBuilder : Builder<WorkflowTaskInstance, Service.Airflow.Model.AirflowTaskInstance>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowTaskInstanceBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowTaskInstanceBuilder> logger) : base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowTaskInstanceBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override Task<List<WorkflowTaskInstance>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskInstance> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowTaskInstance)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowTaskInstance>().ToList());

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

				results.Add(m);
			}

			return Task.FromResult(results);
		}
	}
}
