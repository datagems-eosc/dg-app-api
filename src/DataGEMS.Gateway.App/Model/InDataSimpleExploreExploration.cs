using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class InDataSimpleExploreExploration
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";

		public String Question { get; set; }
		public String SqlPattern { get; set; }
		public List<SimpleQueryInputParam> InputParams { get; set; }
		public String Reasoning { get; set; }
		public String SqlQuery { get; set; }
		public SimpleQuerySqlResults SqlResults { get; set; }
	}

	public class SimpleQueryInputParam
	{
		public decimal Lon { get; set; }

		public decimal Lat { get; set; }
	}

	public class SimpleQuerySqlResults
	{
		public String Status { get; set; }

		public List<Dictionary<String, object>> Data { get; set; }
	}
}
