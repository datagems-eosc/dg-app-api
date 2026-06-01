using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class LinguisticFeaturesRequest
	{
		[JsonProperty("question")]
		public string Question { get; set; }

		[JsonProperty("rag_output")]
		public Discovery.Model.CorpusAnalysisResponse RagOutput { get; set; }

		[JsonProperty("requested_features")]
		public List<string> RequestedFeatures { get; set; }
	}
}
