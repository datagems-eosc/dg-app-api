using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Common
{
	public class AnalyticalPatternNode
	{
		[JsonProperty("id")]
		public Guid Id { get; set; }
		[JsonProperty("labels")]
		public List<string> Labels { get; set; }
		[JsonProperty("properties")]
		public Dictionary<string, object> Properties { get; set; }
	}
}
