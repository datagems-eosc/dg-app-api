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
	public class WorkflowTaskLogsBuilder : Builder<WorkflowTaskLogs, Service.Airflow.Model.AirflowTaskLogs>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowTaskLogsBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowTaskLogsBuilder> logger) : base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowTaskLogsBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override Task<List<WorkflowTaskLogs>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskLogs> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowTaskLogs)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowTaskLogs>().ToList());

			List<WorkflowTaskLogs> results = new List<WorkflowTaskLogs>();

			foreach (Service.Airflow.Model.AirflowTaskLogs d in datas)
			{
				WorkflowTaskLogs m = new WorkflowTaskLogs();

				if (fields.HasField(nameof(WorkflowTaskLogs.Timestamp))) m.Timestamp = d.Timestamp;
				if (fields.HasField(nameof(WorkflowTaskLogs.Event))) m.Event = d.Event;


				results.Add(m);
			}

			return Task.FromResult(results);
		}
	}
}
