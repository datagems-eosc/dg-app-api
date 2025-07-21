using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class ExplorationSimpleExploreResponse
	{
		[JsonProperty("question")]
		public String Question { get; set; }

		[JsonProperty("sql_pattern")]
		public String SqlPattern { get; set; }

		[JsonProperty("input_params")]
		public List<SimpleQueryInputParam> InputParams { get; set; }

		[JsonProperty("reasoning")]
		public String Reasoning { get; set; }

		[JsonProperty("sql_query")]
		public String SqlQuery { get; set; }

		[JsonProperty("sql_results")]
		public SimpleQuerySqlResults SqlResults { get; set; }
	}

	public class SimpleQueryInputParam
	{
		[JsonProperty("lon")]
		public decimal Lon { get; set; }

		[JsonProperty("lat")]
		public decimal Lat { get; set; }
	}

	public class SimpleQuerySqlResults
	{
		[JsonProperty("status")]
		public String Status { get; set; }

		[JsonProperty("data")]
		public List<Dictionary<string, object>> Data { get; set; } 
	}
}
