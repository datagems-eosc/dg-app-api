using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class ExplorationTextToSqlResponse
	{
		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("params")]
		public Params Params { get; set; }

		[JsonProperty("question")]
		public string Question { get; set; }

		[JsonProperty("model_name")]
		public string ModelName { get; set; }

		[JsonProperty("sql_pattern")]
		public string SqlPattern { get; set; }

		[JsonProperty("input_params")]
		public List<InputParam> InputParams { get; set; }

		[JsonProperty("output_params")]
		public OutputParams OutputParams { get; set; }

		[JsonProperty("reasoning")]
		public string Reasoning { get; set; }

		[JsonProperty("sql_query")]
		public string SqlQuery { get; set; }

		[JsonProperty("sql_results")]
		public SqlResults SqlResults { get; set; }
	}

	public class Params
	{
		[JsonProperty("results")]
		public Results Results { get; set; }
	}

	public class Results
	{
		[JsonProperty("points")]
		public List<List<decimal>> Points { get; set; }
	}

	public class InputParam
	{
		[JsonProperty("coordinates")]
		public List<CoordinateTuple> Coordinates { get; set; }
	}

	public class CoordinateTuple
	{
		[JsonProperty("tuple")]
		public List<decimal> Tuple { get; set; }
	}

	public class OutputParams
	{
		[JsonProperty("coordinates")]
		public List<string> Coordinates { get; set; }
	}

	public class SqlResults
	{
		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
