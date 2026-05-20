using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DatasetRecommender.Model
{
	public class MatheRecommendationRequest
	{
		[JsonProperty("question_id")]
		public string QuestionId { get; set; }
		[JsonProperty("question")]
		public string Question { get; set; }
		[JsonProperty("n")]
		public int RecommendedMaterialsCount { get; set; }
	}
}
