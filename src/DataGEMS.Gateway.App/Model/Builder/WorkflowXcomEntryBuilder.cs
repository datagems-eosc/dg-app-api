using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	public class WorkflowXcomEntryBuilder : Builder<WorkflowXcomEntry, Service.Airflow.Model.AirflowXcomEntry>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowXcomEntryBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowXcomEntryBuilder> logger) : base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowXcomEntryBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override Task<List<WorkflowXcomEntry>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowXcomEntry> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowXcomEntry)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowXcomEntry>().ToList());

			List<WorkflowXcomEntry> results = new List<WorkflowXcomEntry>();

			foreach (Service.Airflow.Model.AirflowXcomEntry d in datas)
			{
				WorkflowXcomEntry m = new WorkflowXcomEntry();

				if (fields.HasField(nameof(WorkflowXcomEntry.Key))) m.Key = d.Key;
				if (fields.HasField(nameof(WorkflowXcomEntry.WorkflowId))) m.WorkflowId = d.DagId;
				if (fields.HasField(nameof(WorkflowXcomEntry.WorkflowTaskId))) m.WorkflowTaskId = d.TaskId;
				if (fields.HasField(nameof(WorkflowXcomEntry.WorkflowExecutionId))) m.WorkflowExecutionId = d.DagRunId;
				if (fields.HasField(nameof(WorkflowXcomEntry.MapIndex))) m.MapIndex = d.MapIndex;
				if (fields.HasField(nameof(WorkflowXcomEntry.LogicalDate))) m.LogicalDate = d.LogicalDate;


				results.Add(m);
			}

			return Task.FromResult(results);
		}
	}
}
