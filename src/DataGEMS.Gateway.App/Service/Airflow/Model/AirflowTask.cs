using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskList
	{
		[JsonProperty("tasks")]
		public List<AirflowTask> Items { get; set; }

		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }
	}

	public class AirflowTask
	{
		[JsonProperty("task_id")]
		public String TaskId { get; set; }

		[JsonProperty("task_display_name")]
		public String TaskDisplayName { get; set; }

		[JsonProperty("owner")]
		public String Owner { get; set; }

		[JsonProperty("start_date")]
		public DateTime? Start { get; set; }

		[JsonProperty("end_date")]
		public DateTime? End { get; set; }

		[JsonProperty("trigger_rule")]
		public String TriggerRule { get; set; }

		[JsonProperty("depends_on_past")]
		public Boolean? DependsOnPast { get; set; }

		[JsonProperty("wait_for_downstream")]
		public Boolean? WaitForDownstream { get; set; }

		[JsonProperty("retries")]
		public decimal? Retries { get; set; }

		[JsonProperty("queue")]
		public String Queue { get; set; }

		[JsonProperty("pool")]
		public String Pool { get; set; }

		[JsonProperty("pool_slots")]
		public decimal? PoolSlots { get; set; }

		[JsonProperty("execution_timeout")]
		public Object ExecutionTimeout { get; set; }

		[JsonProperty("retry_delay")]
		public Object RetryDelay { get; set; }

		[JsonProperty("retry_exponential_backoff")]
		public Boolean? RetryExponentialBackoff { get; set; }

		[JsonProperty("priority_weight")]
		public decimal? PriorityWeight { get; set; }

		[JsonProperty("weight_rule")]
		public String WeightRule { get; set; }

		[JsonProperty("ui_color")]
		public String UIColor { get; set; }

		[JsonProperty("ui_fgcolor")]
		public String UIFGColor { get; set; }

		[JsonProperty("template_fields")]
		public List<String> TemplateFields { get; set; }

		[JsonProperty("downstream_task_ids")]
		public List<String> DownstreamTaskIds { get; set; }

		[JsonProperty("doc_md")]
		public String DocMd { get; set; }

		[JsonProperty("operator_name")]
		public String OperatorName { get; set; }

		[JsonProperty("params")]
		public Object Params { get; set; }

		[JsonProperty("class_ref")]
		public Object ClassRef { get; set; }

		[JsonProperty("is_mapped")]
		public Boolean? IsMapped { get; set; }

		[JsonProperty("extra_links")]
		public List<String> ExtraLinks { get; set; }
	}
}
