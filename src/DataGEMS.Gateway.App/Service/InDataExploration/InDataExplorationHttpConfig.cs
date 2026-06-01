
using DataGEMS.Gateway.App.Common.Enum;

namespace DataGEMS.Gateway.App.Service.InDataExploration
{
	public class InDataExplorationHttpConfig
	{
		public String Scope { get; set; }
		public String BaseUrl { get; set; }
		public String ExploreEndpoint { get; set; }
		public string LinguisticFeaturesEndpoint { get; set; }
		public Dictionary<LinguisticFeature, String> LinguisticFeatureMap { get; set; }
	}
}
