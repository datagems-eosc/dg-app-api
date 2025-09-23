using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowDagExecutionList
	{
		[JsonProperty("dag_runs")]
		public List<AirflowDagExecution> Items { get; set; }

		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }
	}

	public class AirflowDagExecutionListRequest
	{
		[JsonProperty("dag_ids")]
		public List<string> DagIds { get; set; }

		[JsonProperty("states")]
		public List<string> States { get; set; }

		[JsonProperty("run_after_gte")]
		public DateTime? RunAfterGte { get; set; }

		[JsonProperty("run_after_lte")]
		public DateTime? RunAfterLte { get; set; }

		[JsonProperty("logical_date_gte")]
		public DateTime? LogicalDateGte { get; set; }

		[JsonProperty("logical_date_lte")]
		public DateTime? LogicalDateLte { get; set; }

		[JsonProperty("start_date_gte")]
		public DateTime? StartDateGte { get; set; }

		[JsonProperty("start_date_lte")]
		public DateTime? StartDateLte { get; set; }

		[JsonProperty("end_date_gte")]
		public DateTime? EndDateGte { get; set; }

		[JsonProperty("end_date_lte")]
		public DateTime? EndDateLte { get; set; }

		[JsonProperty("page_offset")]
		public int? Offset { get; set; }

		[JsonProperty("page_limit")]
		public int? Limit { get; set; }

		[JsonProperty("order_by")]
		public string OrderBy { get; set; }
	}

	public class AirflowDagExecution
	{
		[JsonProperty("dag_run_id")]
		public String RunId { get; set; }
		[JsonProperty("dag_id")]
		public String DagId { get; set; }
		[JsonProperty("queued_at")]
		public DateTime? QueuedAt { get; set; }
		[JsonProperty("start_date")]
		public DateTime? Start { get; set; }
		[JsonProperty("end_date")]
		public DateTime? End { get; set; }
		[JsonProperty("dag_interval_start")]
		public DateTime? IntervalStart { get; set; }
		[JsonProperty("data_interval_end")]
		public DateTime? IntervalEnd { get; set; }
		[JsonProperty("logical_date")]
		public DateTime? LogicalDate { get; set; }
		[JsonProperty("run_after")]
		public DateTime? RunAfter { get; set; }
		[JsonProperty("last_scheduling_decision")]
		public DateTime? LastSchedulingDecision { get; set; }
		[JsonProperty("run_type")]
		public String RunType { get; set; }
		[JsonProperty("state")]
		public String State { get; set; }
		[JsonProperty("triggered_by")]
		public String TriggeredBy { get; set; }
		[JsonProperty("note")]
		public String Note { get; set; }
		[JsonProperty("dag_versions")]
		public Object DagVersions { get; set; }
		[JsonProperty("conf")]
		public Object Conf { get; set; }
		[JsonProperty("bundle_version")]
		public String BundleVersion { get; set; }

	}
}
