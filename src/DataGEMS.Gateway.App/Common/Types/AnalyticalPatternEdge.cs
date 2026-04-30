using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Common
{
	public class AnalyticalPatternEdge
	{
		[JsonProperty("from")]
		public Guid From { get; set; }

		[JsonProperty("to")]
		public Guid To { get; set; }

		[JsonProperty("labels")]
		public List<string> Labels { get; set; }
		[JsonProperty("properties")]
		public Dictionary<string, object> Properties { get; set; }
	}
}
