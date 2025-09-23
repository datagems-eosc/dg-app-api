using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowXcomEntryList
	{
		[JsonProperty("xcom_entries")]
		public List<AirflowXcomEntry> Items { get; set; }

		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }
	}

	public class AirflowXcomEntry
	{
		[JsonProperty("key")]
		public String Key { get; set; }

		[JsonProperty("timestamp")]
		public DateTime? Timestamp { get; set; }

		[JsonProperty("logical_date")]
		public DateTime? LogicalDate { get; set; }

		[JsonProperty("map_index")]
		public int? MapIndex { get; set; }

		[JsonProperty("task_id")]
		public String TaskId { get; set; }

		[JsonProperty("dag_id")]
		public String DagId { get; set; }

		[JsonProperty("run_id")]
		public String DagRunId { get; set; }

		[JsonProperty("value")]
		public String Value { get; set; }
	}
}
