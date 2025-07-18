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

		[JsonProperty("dataset_id")]
		public Guid DatasetId { get; set; }

		[JsonProperty("object_id")]
        public string ObjectId { get; set; }

        [JsonProperty("similarity")]
        public Decimal Similarity { get; set; }
    }
}
