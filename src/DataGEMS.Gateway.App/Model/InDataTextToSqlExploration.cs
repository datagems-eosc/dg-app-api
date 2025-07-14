using DataGEMS.Gateway.App.Service.InDataExploration.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	/*public class InDataTextToSqlExploration
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Sql { get; set; }
	}*/

	public class InDataTextToSqlExploration
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";

		public string Status { get; set; }
		public string Message { get; set; }
		public Params Params { get; set; }
		public string Question { get; set; }
		public string ModelName { get; set; }
		public string SqlPattern { get; set; }
		public List<InputParam> InputParams { get; set; }
		public OutputParams OutputParams { get; set; }
		public string Reasoning { get; set; }
		public string SqlQuery { get; set; }
		public SqlResults SqlResults { get; set; }
	}

	public class Params
	{
		public Results Results { get; set; }
	}

	public class Results
	{
		public List<List<decimal>> Points { get; set; }
	}

	public class InputParam
	{
		public List<CoordinateTuple> Coordinates { get; set; }
	}

	public class CoordinateTuple
	{
		public List<decimal> Tuple { get; set; }
	}

	public class OutputParams
	{
		public List<string> Coordinates { get; set; }
	}

	public class SqlResults
	{
		public string Status { get; set; }
		public string Message { get; set; }
	}
}
