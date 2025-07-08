using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Discovery.Model
{
    public class CrossDatasetDiscoveryResponse
    {
        [JsonProperty("query_time")]
        public double QueryTime { get; set; }

        [JsonProperty("results")]
        public List<CrossDatasetDiscoveryResult> Results { get; set; }
    }

    public class CrossDatasetDiscoveryResult
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("use_case")]
        public string UseCase { get; set; }

        [JsonProperty("source")]
		public Guid Source { get; set; } //DatasetId

		[JsonProperty("source_id")]
        public string SourceId { get; set; }

        [JsonProperty("chunk_id")]
        public string ChunkId { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("distance")]
        public Decimal Distance { get; set; }
    }
}
