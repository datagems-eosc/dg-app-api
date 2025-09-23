using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskLogList
	{
		[JsonProperty("content")]
		public List<AirflowTaskLog> Content { get; set; }
		[JsonProperty("continuation_token")]
		public String ContinuationToken { get; set; }
	}

	public class AirflowTaskLog
	{
		[JsonProperty("timestamp")]
		public DateTime? Timestamp { get; set; }
		[JsonProperty("event")]
		public String Event { get; set; }
	}
}
