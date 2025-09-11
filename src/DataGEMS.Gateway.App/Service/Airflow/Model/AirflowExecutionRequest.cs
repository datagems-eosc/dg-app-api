using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowListExecutionRequest
	{
		[JsonProperty("dag_id")]
		public String DagId { get; set; }
		[JsonProperty("limit")]
		public int? Limit { get; set; }
	}

	public class AirflowExecutionRequest
	{
		[JsonProperty("dag_run_id")]
		public string DagRunId { get; set; }
		[JsonProperty("logical_date")]
		public DateTime? LogicalDate { get; set; }
	}


	public class AirflowBatchExecutionRequest
	{

		[JsonProperty("dag_id")]
		public string DagId { get; set; }
		[JsonProperty("order_by")]
		public string? OrderBy { get; set; }

		[JsonProperty("page_offset")]
		public int PageOffset { get; set; } = 0;

		[JsonProperty("page_limit")]
		public int PageLimit { get; set; } = 100;

		[JsonProperty("dag_ids")]
		public List<string>? DagIds { get; set; }

		[JsonProperty("states")]
		public List<string>? States { get; set; }

		[JsonProperty("run_after_gte")]
		public string? RunAfterGte { get; set; }

		[JsonProperty("run_after_lte")]
		public string? RunAfterLte { get; set; }

		[JsonProperty("logical_date_gte")]
		public string? LogicalDateGte { get; set; }

		[JsonProperty("logical_date_lte")]
		public string? LogicalDateLte { get; set; }

		[JsonProperty("start_date_gte")]
		public string? StartDateGte { get; set; }
	}

}
