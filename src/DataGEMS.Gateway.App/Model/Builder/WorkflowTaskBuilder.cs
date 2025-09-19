using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Service.Airflow.Model;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowTaskBuilder : Builder<WorkflowTasks, Service.Airflow.Model.AirflowTaskExecution>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowTaskBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowTaskBuilder> logger) : base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowTaskBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override Task<List<WorkflowTasks>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskExecution> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowTaskExecution)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowTasks>().ToList());

			List<WorkflowTasks> results = new List<WorkflowTasks>();

			foreach (Service.Airflow.Model.AirflowTaskExecution d in datas)
			{
				WorkflowTasks m = new WorkflowTasks();

				if (fields.HasField(nameof(WorkflowTasks.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(WorkflowTasks.DagId))) m.DagId = d.DagId;
				if (fields.HasField(nameof(WorkflowTasks.TaskId))) m.TaskId = d.TaskId;
				if (fields.HasField(nameof(WorkflowTasks.DagRunId))) m.DagRunId = d.DagRunId;
				if (fields.HasField(nameof(WorkflowTasks.LogicalDate))) m.LogicalDate = d.LogicalDate;
				if (fields.HasField(nameof(WorkflowTasks.Start))) m.Start = d.Start;
				if (fields.HasField(nameof(WorkflowTasks.End))) m.End = d.End;
				if (fields.HasField(nameof(WorkflowTasks.Duration))) m.Duration = d.Duration;
				if (fields.HasField(nameof(WorkflowTasks.State))) m.State = d.State;
				if (fields.HasField(nameof(WorkflowTasks.TryNumber))) m.TryNumber = d.TryNumber;
				if (fields.HasField(nameof(WorkflowTasks.MaxTries))) m.MaxTries = d.MaxTries;
				if (fields.HasField(nameof(WorkflowTasks.TaskDisplayName))) m.TaskDisplayName = d.TaskDisplayName;
				if (fields.HasField(nameof(WorkflowTasks.Hostname))) m.Hostname = d.Hostname;
				if (fields.HasField(nameof(WorkflowTasks.Unixname))) m.Unixname = d.Unixname;
				if (fields.HasField(nameof(WorkflowTasks.PoolSlots))) m.PoolSlots = d.PoolSlots;
				if (fields.HasField(nameof(WorkflowTasks.Queue))) m.Queue = d.Queue;
				if (fields.HasField(nameof(WorkflowTasks.QueuedWhen))) m.QueuedWhen = d.QueuedWhen;
				if (fields.HasField(nameof(WorkflowTasks.PriorityWeight))) m.PriorityWeight = d.PriorityWeight;
				if (fields.HasField(nameof(WorkflowTasks.Operator))) m.Operator = d.Operator;
				if (fields.HasField(nameof(WorkflowTasks.ScheduledWhen))) m.ScheduledWhen = d.ScheduledWhen;
				if (fields.HasField(nameof(WorkflowTasks.Pid))) m.Pid = d.Pid;
				if (fields.HasField(nameof(WorkflowTasks.Executor))) m.Executor = d.Executor;
				if (fields.HasField(nameof(WorkflowTasks.ExecutorConfig))) m.ExecutorConfig = d.ExecutorConfig;
				if (fields.HasField(nameof(WorkflowTasks.Note))) m.Note = d.Note;
				if (fields.HasField(nameof(WorkflowTasks.RenderedMapIndex))) m.RenderedMapIndex = d.RenderedMapIndex;
				 if (fields.HasField(nameof(WorkflowTasks.RenderedFields))) m.RenderedFields = d.RenderedFields;
				if (fields.HasField(nameof(WorkflowTasks.Trigger))) m.Trigger = d.Trigger;
				if (fields.HasField(nameof(WorkflowTasks.TriggererJob))) m.TriggererJob = d.TriggererJob;
				if (fields.HasField(nameof(WorkflowTasks.DagVersion))) m.DagVersion = d.DagVersion;

				results.Add(m);
			}

			return Task.FromResult(results);
		}
	}
}
