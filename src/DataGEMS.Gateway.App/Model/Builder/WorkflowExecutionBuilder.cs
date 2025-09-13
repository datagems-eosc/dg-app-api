using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowExecutionBuilder : Builder<WorkflowExecution, Service.Airflow.Model.AirflowDagExecution>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowExecutionBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowExecutionBuilder> logger): base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowExecutionBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override Task<List<WorkflowExecution>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDagExecution> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowDagExecution)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowExecution>().ToList());

			List<WorkflowExecution> results = new List<WorkflowExecution>();

			foreach (Service.Airflow.Model.AirflowDagExecution d in datas)
			{
				WorkflowExecution m = new WorkflowExecution();

				if (fields.HasField(nameof(WorkflowExecution.Id))) m.Id=d.RunId;
				if (fields.HasField(nameof(WorkflowExecution.WorkflowId))) m.WorkflowId = d.Id;
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

				results.Add(m);
			}

			return Task.FromResult(results);
		}
	}
}
