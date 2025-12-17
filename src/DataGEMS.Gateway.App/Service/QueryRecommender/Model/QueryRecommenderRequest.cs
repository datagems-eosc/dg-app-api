using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.QueryRecommender.Model
{
	public class QueryRecommenderRequest
	{
		[JsonProperty("current_query")]
		public string CurrentQuery { get; set; }
		[JsonProperty("context")]
		public InternalContext Context { get; set; }

		public class InternalContext
		{
			[JsonProperty("user_id")]
			public string UserId { get; set; }
			[JsonProperty("results")]
			public List<object> Results { get; set; }
		}
	}
}
