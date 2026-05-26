using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Discovery.Model
{
	public class CorpusAnalysisResponse
	{
		[JsonProperty("query_time")]
		public double QueryTime { get; set; }

		[JsonProperty("results")]
		public List<CorpusAnalysisResponse.CorpusAnalysisResult> Results { get; set; }

		public class CorpusAnalysisResult
		{
			[JsonProperty("content")]
			public string Content { get; set; }
			[JsonProperty("dataset_id")]
			public Guid DatasetId { get; set; }
			[JsonProperty("object_id")]
			public Guid ObjectId { get; set; }
			[JsonProperty("similarity")]
			public double Similarity { get; set; }
			[JsonProperty("metadata")]
			public Dictionary<string, object> Metadata { get; set; }
		}
	}
}
