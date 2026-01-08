using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskInstanceList
	{
		[JsonProperty("task_instances")]
		public List<AirflowTaskInstance> Items { get; set; }

		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }
	}

	public class ClearTaskInstanceResponse
	{
		[JsonProperty("task_instances")]
		public List<AirflowTaskInstance> TaskInstances { get; set; }
	}

	public class AirflowTaskInstanceListRequest
	{
		[JsonProperty("dag_ids", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> DagIds { get; set; }

		[JsonProperty("dag_run_ids", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> DagRunIds { get; set; }

		[JsonProperty("task_ids", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> TaskIds { get; set; }

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> State { get; set; }

		[JsonProperty("run_after_gte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? RunAfterGte { get; set; }

		[JsonProperty("run_after_lte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? RunAfterLte { get; set; }

		[JsonProperty("logical_date_gte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? LogicalDateGte { get; set; }

		[JsonProperty("logical_date_lte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? LogicalDateLte { get; set; }

		[JsonProperty("start_date_gte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? StartDateGte { get; set; }

		[JsonProperty("start_date_lte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? StartDateLte { get; set; }

		[JsonProperty("end_date_gte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? EndDateGte { get; set; }

		[JsonProperty("end_date_lte", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? EndDateLte { get; set; }

		[JsonProperty("duration_gte", NullValueHandling = NullValueHandling.Ignore)]
		public Decimal? DurationGte { get; set; }

		[JsonProperty("duration_lte", NullValueHandling = NullValueHandling.Ignore)]
		public Decimal? DurationLte { get; set; }

		[JsonProperty("pool", NullValueHandling = NullValueHandling.Ignore)]
		public List<String> Pool { get; set; }

		[JsonProperty("queue", NullValueHandling = NullValueHandling.Ignore)]
		public List<String> Queue { get; set; }

		[JsonProperty("executor", NullValueHandling = NullValueHandling.Ignore)]
		public List<String> Executor { get; set; }

		[JsonProperty("page_offset", NullValueHandling = NullValueHandling.Ignore)]
		public int? Offset { get; set; }

		[JsonProperty("page_limit", NullValueHandling = NullValueHandling.Ignore)]
		public int? Limit { get; set; }

		[JsonProperty("order_by", NullValueHandling = NullValueHandling.Ignore)]
		public string OrderBy { get; set; }
	}

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
		public String Queue { get; set; }
		[JsonProperty("priority_weight")]
		public int? PriorityWeight { get; set; }
		[JsonProperty("operator")]
		public String Operator { get; set; }
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
