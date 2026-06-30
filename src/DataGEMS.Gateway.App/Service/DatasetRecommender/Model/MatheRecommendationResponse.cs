using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DatasetRecommender.Model
{
	public class MatheRecommendationResponse
	{
		[JsonProperty("question_id")]
		public string QuestionId { get; set; }

		[JsonProperty("recommendations")]
		public List<MatheRecommendationResponse.Recommendation> Recommendations { get; set; }

		//GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";


		public class Recommendation
		{
			[JsonProperty("material_id")]
			public string MaterialId { get; set; }
		}
	}
}
