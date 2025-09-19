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
}
