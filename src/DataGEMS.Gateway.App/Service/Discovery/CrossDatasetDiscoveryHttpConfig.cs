
namespace DataGEMS.Gateway.App.Service.Discovery
{
	public class CrossDatasetDiscoveryHttpConfig
	{
		public String Scope { get; set; }
		public String BaseUrl { get; set; }
		public String SearchEndpoint {  get; set; }
		public string CorpusAnalysisEndpoint { get; set; }
		public int DefaultResultCount { get; set; }
		public bool UseTaskOrcherstrator { get; set; }
	}
}
