using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class ExplorationTextToSqlRequest
	{
		[JsonProperty("question")]
		public string Question { get; set; }

		[JsonProperty("parameters")]
		public SqlQueryParameters Parameters { get; set; }
	}

	public class SqlQueryParameters
	{
		[JsonProperty("results")]
		public SqlQueryResults Results { get; set; }
	}

	public class SqlQueryResults
	{
		[JsonProperty("points")]
		public List<List<decimal>> Points { get; set; }
	}
}
