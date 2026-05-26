using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.InDatasetDiscovery.Model
{
	public class LinguisticFeaturesRequest
	{
		[JsonProperty("question")]
		public string Question { get; set; }

		[JsonProperty("rag_output")]
		public Service.Discovery.Model.CorpusAnalysisResponse RagOutput { get; set; }
	}
}
