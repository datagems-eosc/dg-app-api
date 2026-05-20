using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DatasetRecommender.Model
{
	public class DatasetRecommendationResponse
	{
		[JsonProperty("entity_id")]
		public Guid EntityId { get; set; }

		[JsonProperty("recommendations")]
		public List<DatasetRecommendationResponse.Recommendation> Recommendations { get; set; }


		public class Recommendation
		{
			[JsonProperty("entity_id")]
			public Guid DatasetId { get; set; }
		}
	}
}
