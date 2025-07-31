using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class AirflowBuilder : Builder<Airflow,Service.Airflow.Model.AirflowDagItem>
	{
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly ILogger<AirflowBuilder> _logger;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public AirflowBuilder(
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<AirflowBuilder> logger): base(logger)
		{
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
			this._logger = logger;
		}

		public AirflowBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override async Task<List<Airflow>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDagItem> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowDagItem)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any())
				return Enumerable.Empty<Airflow>().ToList();

			List<Airflow> results = new List<Airflow>();

			foreach (var d in datas)
			{
				var m = new Airflow();

				if (fields.HasField(nameof(Airflow.Dag_Id))) m.Dag_Id = d.Dag_Id;
				if (fields.HasField(nameof(Airflow.Dag_Name))) m.Dag_Name = d.Dag_Name;


				results.Add(m);
			}

			return results;
		}
		//public Airflow Build(DataGEMS.Gateway.App.Model.Airflow data, FieldSet fields)
		//{
		//	return BuildList(new[] { data }, fields).FirstOrDefault();
		//}
		//public List<Airflow> BuildList(IEnumerable<DataGEMS.Gateway.App.Model.Airflow> data, FieldSet fields)
		//{
		//	if (fields == null || fields.IsEmpty() || data == null || !data.Any())
		//		return new List<Airflow>();

		//	return data.Select(d =>
		//	{
		//		var builder = new Airflow();

		//		if (fields.HasField(nameof(Airflow.Dag_Id))) builder.Dag_Id = d.Dag_Id;
		//		if (fields.HasField(nameof(Airflow.Dag_Name))) builder.Dag_Name = d.Dag_Name;


		//		return builder;
		//	}).ToList();
		//}
	}
}
