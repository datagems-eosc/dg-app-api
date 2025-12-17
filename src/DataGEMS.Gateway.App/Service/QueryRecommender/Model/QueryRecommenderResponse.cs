using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.QueryRecommender.Model
{
	public class QueryRecommenderResponse
	{
		[JsonProperty("next_queries")]
		public List<string> NextQueries { get; set; }
	}
}
