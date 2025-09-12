using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowDagExecutionListRequest
	{
		[JsonProperty("dag_ids")]
		public List<string> DagIds { get; set; }

		[JsonProperty("order_by")]
		public string OrderBy { get; set; }

		[JsonProperty("page_offset")]
		public int Offset { get; set; }

		[JsonProperty("page_limit")]
		public int Limit { get; set; }

		[JsonProperty("states")]
		public List<string> States { get; set; }

		[JsonProperty("run_after_gte")]
		public DateTime RunAfterGte { get; set; }

		[JsonProperty("run_after_lte")]
		public DateTime RunAfterLte { get; set; }

		[JsonProperty("logical_date_gte")]
		public DateTime LogicalDateGte { get; set; }

		[JsonProperty("logical_date_lte")]
		public DateTime LogicalDateLte { get; set; }

		[JsonProperty("start_date_gte")]
		public DateTime StartDateGte { get; set; }

		[JsonProperty("start_date_lte")]
		public DateTime StartDateLte { get; set; }

		[JsonProperty("end_date_gte")]
		public DateTime EndDateGte { get; set; }

		[JsonProperty("end_date_lte")]
		public DateTime EndDateLte { get; set; }
	}

}
