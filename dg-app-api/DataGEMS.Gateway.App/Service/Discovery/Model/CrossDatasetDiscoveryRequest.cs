using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Discovery.Model
{
	public class CrossDatasetDiscoveryRequest
	{
		[JsonProperty("query")]
		public string Query { get; set; }
		[JsonProperty("k")]
		public int ResultCount { get; set; }
	}
}
