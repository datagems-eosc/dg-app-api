using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class InDataExplorationResponse
	{
		[JsonProperty("question")]
		public String Question { get; set; }

		[JsonProperty("sql_pattern")]
		public String SqlPattern { get; set; }

		[JsonProperty("input_params")]
		//public List<ExploreInputParam> InputParams { get; set; }
		public Object InputParams { get; set; }

		[JsonProperty("reasoning")]
		public String Reasoning { get; set; }

		[JsonProperty("sql_query")]
		public String SqlQuery { get; set; }

		[JsonProperty("sql_results")]
		public ExploreSqlResults SqlResults { get; set; }

		//public class ExploreInputParam
		//{
		//	[JsonProperty("lon")]
		//	public decimal Lon { get; set; }

		//	[JsonProperty("lat")]
		//	public decimal Lat { get; set; }
		//}

		public class ExploreSqlResults
		{
			[JsonProperty("status")]
			public String Status { get; set; }

			[JsonProperty("message")]
			public String Message { get; set; }

			[JsonProperty("data")]
			public List<Dictionary<string, object>> Data { get; set; }
		}
	}
}
