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
}
