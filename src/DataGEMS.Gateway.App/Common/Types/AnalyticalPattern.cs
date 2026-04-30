using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Common
{
	public class AnalyticalPattern
	{
		[JsonProperty("edges")]
		public List<AnalyticalPatternEdge> Edges { get; set; }
		[JsonProperty("nodes")]
		public List<AnalyticalPatternNode> Nodes { get; set; }
	}
}
