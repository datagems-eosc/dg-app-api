using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowAccessToken
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }
	}
}
