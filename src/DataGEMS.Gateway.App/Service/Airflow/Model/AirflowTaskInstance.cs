using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskInstance
	{
		[JsonProperty("id")]
		public String Id { get; set; }
		[JsonProperty("task_id")]
		public String TaskId { get; set; }
		[JsonProperty("dag_run_id")]
		public String DagRunId { get; set; }
		[JsonProperty("dag_id")]
		public String DagId { get; set; }

		[JsonProperty("map_index")]
		public String MapIndex { get; set; }
		[JsonProperty("logical_date")]
		public DateTime? LogicalDate { get; set; }
		[JsonProperty("run_after")]
		public DateTime? RunAfter { get; set; }
		[JsonProperty("start_date")]
		public DateTime? Start { get; set; }
		[JsonProperty("end_date")]
		public DateTime? End { get; set; }
		[JsonProperty("duration")]
		public Decimal? Duration { get; set; }

		[JsonProperty("state")]
		public String State { get; set; }
		[JsonProperty("try_number")]
		public int? TryNumber { get; set; }
		[JsonProperty("max_tries")]
		public int? MaxTries { get; set; }
		[JsonProperty("task_display_name")]
		public String TaskDisplayName { get; set; }
		[JsonProperty("hostname")]
		public String Hostname { get; set; }
		[JsonProperty("unixname")]
		public String Unixname { get; set; }
		[JsonProperty("pool")]
		public String Pool { get; set; }
		[JsonProperty("pool_slots")]
		public int PoolSlots { get; set; }
		[JsonProperty("queue")]
		public String? Queue { get; set; }
		[JsonProperty("priority_weight")]
		public int? PriorityWeight { get; set; }
		[JsonProperty("operator")]
		public String? Operator { get; set; }
		[JsonProperty("queued_when")]
		public DateTime? QueuedWhen { get; set; }
		[JsonProperty("scheduled_when")]
		public DateTime? ScheduledWhen { get; set; }
		[JsonProperty("pid")]
		public int? Pid { get; set; }
		[JsonProperty("executor")]
		public String Executor { get; set; }
		[JsonProperty("executor_config")]
		public String ExecutorConfig { get; set; }
		[JsonProperty("note")]
		public String Note { get; set; }
		[JsonProperty("rendered_map_index")]
		public String RenderedMapIndex { get; set; }
		[JsonProperty("rendered_fields")]
		public Object RenderedFields { get; set; }
		[JsonProperty("trigger")]
		public Object Trigger { get; set; }
		[JsonProperty("triggerer_job")]
		public Object TriggererJob { get; set; }
		[JsonProperty("dag_versions")]
		public Object DagVersion { get; set; }
	}
}
