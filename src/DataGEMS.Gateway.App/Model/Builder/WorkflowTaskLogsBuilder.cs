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
	public class WorkflowTaskLogBuilder : Builder<WorkflowTaskLog, Service.Airflow.Model.AirflowTaskLog>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowTaskLogBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowTaskLogBuilder> logger) : base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowTaskLogBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override Task<List<WorkflowTaskLog>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowTaskLog> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowTaskLog)).And("fields", fields));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowTaskLog>().ToList());

			List<WorkflowTaskLog> results = new List<WorkflowTaskLog>();

			foreach (Service.Airflow.Model.AirflowTaskLog d in datas)
			{
				WorkflowTaskLog m = new WorkflowTaskLog();

				if (fields.HasField(nameof(WorkflowTaskLog.Timestamp))) m.Timestamp = d.Timestamp;
				if (fields.HasField(nameof(WorkflowTaskLog.Event))) m.Event = d.Event;


				results.Add(m);
			}

			return Task.FromResult(results);
		}
	}
}
