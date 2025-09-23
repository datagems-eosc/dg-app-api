using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowTaskBuilder : Builder<WorkflowTask, Service.Airflow.Model.AirflowTask>
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

		public override Task<List<WorkflowTask>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTask> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowTask)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowTask>().ToList());

			List<WorkflowTask> results = new List<WorkflowTask>();

			foreach (Service.Airflow.Model.AirflowTask d in datas)
			{
				WorkflowTask m = new WorkflowTask();

				if (fields.HasField(nameof(WorkflowTask.Id))) m.Id = d.TaskId;
				if (fields.HasField(nameof(WorkflowTask.TaskDisplayName))) m.TaskDisplayName = d.TaskDisplayName;
				if (fields.HasField(nameof(WorkflowTask.Owner))) m.Owner = d.Owner;
				if (fields.HasField(nameof(WorkflowTask.Start))) m.Start = d.Start;
				if (fields.HasField(nameof(WorkflowTask.End))) m.End = d.End;
				if (fields.HasField(nameof(WorkflowTask.TriggerRule))) m.TriggerRule = d.TriggerRule;
				if (fields.HasField(nameof(WorkflowTask.DependsOnPast))) m.DependsOnPast = d.DependsOnPast;
				if (fields.HasField(nameof(WorkflowTask.WaitForDownstream))) m.WaitForDownstream = d.WaitForDownstream;
				if (fields.HasField(nameof(WorkflowTask.Retries))) m.Retries = d.Retries;
				if (fields.HasField(nameof(WorkflowTask.Queue))) m.Queue = d.Queue;
				if (fields.HasField(nameof(WorkflowTask.Pool))) m.Pool = d.Pool;
				if (fields.HasField(nameof(WorkflowTask.PoolSlots))) m.PoolSlots = d.PoolSlots;
				if (fields.HasField(nameof(WorkflowTask.ExecutionTimeout))) m.ExecutionTimeout = d.ExecutionTimeout;
				if (fields.HasField(nameof(WorkflowTask.RetryDelay))) m.RetryDelay = d.RetryDelay;
				if (fields.HasField(nameof(WorkflowTask.RetryExponentialBackoff))) m.RetryExponentialBackoff = d.RetryExponentialBackoff;
				if (fields.HasField(nameof(WorkflowTask.PriorityWeight))) m.PriorityWeight = d.PriorityWeight;
				if (fields.HasField(nameof(WorkflowTask.WeightRule))) m.WeightRule = d.WeightRule;
				if (fields.HasField(nameof(WorkflowTask.UIColor))) m.UIColor = d.UIColor;
				if (fields.HasField(nameof(WorkflowTask.UIFGColor))) m.UIFGColor = d.UIFGColor;
				if (fields.HasField(nameof(WorkflowTask.TemplateFields))) m.TemplateFields = d.TemplateFields;
				if (fields.HasField(nameof(WorkflowTask.DownstreamTaskIds))) m.DownstreamTaskIds = d.DownstreamTaskIds;
				if (fields.HasField(nameof(WorkflowTask.DocMd))) m.DocMd = d.DocMd;
				if (fields.HasField(nameof(WorkflowTask.OperatorName))) m.OperatorName = d.OperatorName;
				if (fields.HasField(nameof(WorkflowTask.Params))) m.Params = d.Params;
				if (fields.HasField(nameof(WorkflowTask.ClassRef))) m.ClassRef = d.ClassRef;
				if (fields.HasField(nameof(WorkflowTask.IsMapped))) m.IsMapped = d.IsMapped;
				if (fields.HasField(nameof(WorkflowTask.ExtraLinks))) m.ExtraLinks = d.ExtraLinks;

				results.Add(m);
			}

			return Task.FromResult(results);
		}
	}
}
