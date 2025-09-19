using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowDagExecution
	{
		[JsonProperty("dag_run_id")]
		public String RunId { get; set; }
		[JsonProperty("dag_id")]
		public String Id { get; set; }
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
