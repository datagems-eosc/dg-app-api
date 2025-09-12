using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowExecutionRequest
	{
		[JsonProperty("dag_run_id")]
		public string DagRunId { get; set; }
		[JsonProperty("logical_date")]
		public DateTime? LogicalDate { get; set; }
	}
}
