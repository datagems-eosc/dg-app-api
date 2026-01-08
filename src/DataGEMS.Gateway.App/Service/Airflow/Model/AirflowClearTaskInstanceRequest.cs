using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowClearTaskInstanceRequest
	{
		[JsonProperty("dry_run")]
		public bool DryRun { get; set; } = true;
		[JsonProperty("start_date")]
		public DateTime? StartDate { get; set; }
		[JsonProperty("end_date")]
		public DateTime? EndDate { get; set; }
		[JsonProperty("only_failed")]
		public bool OnlyFailed { get; set; } = true;
		[JsonProperty("only_running")]
		public bool OnlyRunning { get; set; }
		[JsonProperty("reset_dag_runs")]
		public bool ResetDagRuns { get; set; } = true;
		[JsonProperty("task_ids")]
		public List<string> TaskIds { get; set; }
		[JsonProperty("dag_run_id")]
		public string DagRunId { get; set; }
		[JsonProperty("include_upstream")]
		public bool IncludeUpstream { get; set; }
		[JsonProperty("include_downstream")]
		public bool IncludeDownstream { get; set; }
		[JsonProperty("include_future")]
		public bool IncludeFuture { get; set; }
		[JsonProperty("include_past")]
		public bool IncludePast { get; set; }
		[JsonProperty("run_on_latest_version")]
		public bool RunOnLatestVersion { get; set; }
	}
}
