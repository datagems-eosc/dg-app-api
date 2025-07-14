using Cite.Tools.FieldSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.InDataExploration
{
	public interface IInDataExplorationService
	{
		Task<List<App.Model.InDataGeoQueryExploration>> ExploreGeoQueryAsync(Service.InDataExploration.ExploreGeoQueryInfo request, IFieldSet fieldSet);
		Task<List<App.Model.InDataTextToSqlExploration>> ExploreTextToSqlAsync(Service.InDataExploration.ExploreTextToSqlInfo request, IFieldSet fieldSet);
	}

	public class ExploreGeoQueryInfo
	{
		//GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Question { get; set; }
	}

	public class ExploreTextToSqlInfo
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static string ModelVersion = "V1";
		public string Question { get; set; }
		public Dictionary<string, object> Parameters { get; set; }
	}

	/*public class SqlQueryParametersInfo
	{
		public SqlQueryResultsInfo Results { get; set; }
	}

	public class SqlQueryResultsInfo
	{
		public List<List<decimal>> Points { get; set; }
	}*/
}
