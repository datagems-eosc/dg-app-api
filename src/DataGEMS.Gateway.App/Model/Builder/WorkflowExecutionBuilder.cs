using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
				return Task.FromResult(Enumerable.Empty<WorkflowExecution>().ToList());// here it return to me the count = 0 cause of the if case .... well its gets null fields and the datas are 28 so the issue is within the fields !!!!

			List<WorkflowExecution> results = new List<WorkflowExecution>();

			foreach (Service.Airflow.Model.AirflowDagExecution d in datas)
			{
				WorkflowExecution trigger = new WorkflowExecution();

				if (fields.HasField(nameof(WorkflowExecution.Id))) trigger.Id=d.Id;
				if (fields.HasField(nameof(WorkflowExecution.RunId))) trigger.RunId = d.RunId;
				if (fields.HasField(nameof(WorkflowExecution.QueuedAt))) trigger.QueuedAt = d.QueuedAt;
				if (fields.HasField(nameof(WorkflowExecution.Start))) trigger.Start = d.Start;
				if (fields.HasField(nameof(WorkflowExecution.End))) trigger.End = d.End;
				if (fields.HasField(nameof(WorkflowExecution.IntervalStart))) trigger.IntervalStart = d.IntervalStart;
				if (fields.HasField(nameof(WorkflowExecution.IntervalEnd))) trigger.IntervalEnd = d.IntervalEnd;
				if (fields.HasField(nameof(WorkflowExecution.LogicalDate))) trigger.LogicalDate = d.LogicalDate;
				if (fields.HasField(nameof(WorkflowExecution.RunAfter))) trigger.RunAfter = d.RunAfter;
				if (fields.HasField(nameof(WorkflowExecution.LastSchedulingDecision))) trigger.LastSchedulingDecision = d.LastSchedulingDecision;
				if (fields.HasField(nameof(WorkflowExecution.RunType))) trigger.RunType = d.RunType;
				if (fields.HasField(nameof(WorkflowExecution.TriggeredBy))) trigger.TriggeredBy = d.TriggeredBy;
				if (fields.HasField(nameof(WorkflowExecution.State))) trigger.State = d.State;
				if (fields.HasField(nameof(WorkflowExecution.Note))) trigger.Note = d.Note;
				if (fields.HasField(nameof(WorkflowExecution.DagVersions))) trigger.DagVersions = d.DagVersions;
				if (fields.HasField(nameof(WorkflowExecution.Conf))) trigger.Conf = d.Conf;
				if (fields.HasField(nameof(WorkflowExecution.BundleVersion))) trigger.BundleVersion = d.BundleVersion;
			//	if (fields.HasField(nameof(WorkflowExecution.ListDagIds))) trigger.ListDagIds = d.ListDagIds;

				results.Add(trigger);
			}

			return Task.FromResult(results);
		}
	}
}
