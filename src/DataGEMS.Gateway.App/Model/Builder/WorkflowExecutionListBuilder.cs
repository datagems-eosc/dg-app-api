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
	public class WorkflowListExecutionBuilder : Builder<WorkflowListExecution, Service.Airflow.Model.AirflowDagListExecution>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowListExecutionBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowExecutionBuilder> logger) : base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowListExecutionBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}


		public override Task<List<WorkflowListExecution>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDagListExecution> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowDagListExecution)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Task.FromResult(Enumerable.Empty<WorkflowListExecution>().ToList());

			List<WorkflowListExecution> results = new List<WorkflowListExecution>();

			foreach (Service.Airflow.Model.AirflowDagListExecution d in datas)
			{
				WorkflowListExecution trigger = new WorkflowListExecution();

				if (fields.HasField(nameof(WorkflowListExecution.DagRuns))) trigger.DagRuns = d.DagRuns;
				if (fields.HasField(nameof(WorkflowListExecution.TotalEntries))) trigger.TotalEntries = d.TotalEntries;

				results.Add(trigger);
			}

			return Task.FromResult(results);
		}
	}
}
