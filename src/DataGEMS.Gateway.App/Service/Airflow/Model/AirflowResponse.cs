using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowDagList
	{
		[JsonProperty("dags")]
		public List<AirflowDag> Items { get; set; }
		[JsonProperty("total_entries")]
		public int? TotalEntries { get; set; }
	}

	public class AirflowDag
	{
		[JsonProperty("dag_id")]
		public String Id { get; set; }
		[JsonProperty("dag_display_name")]
		public String Name { get; set; }
		[JsonProperty("is_paused")]
		public Boolean? IsPaused { get; set; }
		[JsonProperty("is_stale")]
		public Boolean? IsStale { get; set; }
		[JsonProperty("last_parsed_time")]
		public DateTime? LastParsedTime { get; set; }
		[JsonProperty("last_expired")]
		public DateTime? LastExpired { get; set; }
		[JsonProperty("bundle_name")]
		public String BundleName { get; set; }
		[JsonProperty("bundle_version")]
		public String BundleVersion { get; set; }
		[JsonProperty("relative_fileloc")]
		public String RelativeFileLocation { get; set; }
		[JsonProperty("fileloc")]
		public String FileLocation { get; set; }
		[JsonProperty("file_token")]
		public String FileToken { get; set; }
		[JsonProperty("description")]
		public String Description { get; set; }
		[JsonProperty("timetable_summary")]
		public String TimetableSummary { get; set; }
		[JsonProperty("timetable_description")]
		public String TimetableDescription { get; set; }
		[JsonProperty("tags")]
		public List<DagTag> Tags { get; set; }
		[JsonProperty("max_active_tasks")]
		public int? MaxActiveTasks { get; set; }
		[JsonProperty("max_active_runs")]
		public int? MaxActiveRuns { get; set; }
		[JsonProperty("max_consecutive_failed_dag_runs")]
		public int? MaxConsecutiveFailedRuns { get; set; }
		[JsonProperty("has_task_concurrency_limits")]
		public Boolean? HasTaskConcurrencyLimits { get; set; }
		[JsonProperty("has_import_errors")]
		public Boolean? HasImportErrors { get; set; }
		[JsonProperty("next_dagrun_logical_date")]
		public DateTime? NextLogicalDate { get; set; }
		[JsonProperty("next_dagrun_data_interval_start")]
		public DateTime? NextDataIntervalStart { get; set; }
		[JsonProperty("next_dagrun_data_interval_end")]
		public DateTime? NextDataIntervalEnd { get; set; }
		[JsonProperty("next_dagrun_run_after")]
		public DateTime? NextRunAfter { get; set; }
		[JsonProperty("owners")]
		public List<String> Owners { get; set; }

		public class DagTag
		{
			[JsonProperty("dag_id")]
			public String DagId { get; set; }
			[JsonProperty("name")]
			public String Name { get; set; }
		}
	}

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
		public List<Object> DagVersions { get; set; }
		[JsonProperty("conf")]
		public Object Conf { get; set; }
		[JsonProperty("bundle_version")]
		public String BundleVersion { get; set; }

	}

	public class AirflowDagListExecution
	{
		[JsonProperty("dag_runs")]
		public List<AirflowDagExecution> DagRuns { get; set; } // the response list is the exact same as the response of the execute/trigger of the dag

		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }
	}

}
