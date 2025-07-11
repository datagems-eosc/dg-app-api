using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class InDataGeoQueryExploration
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Place { get; set; }
		public WikidataInfo MostRelevantWikidata { get; set; }
		public String Oql { get; set; }
		public GeoQueryResults Results { get; set; }
	}

	public class WikidataInfo
	{
		public String Id { get; set; }
		public String Label { get; set; }
		public String Description { get; set; }
	}

	public class GeoQueryResults
	{
		public List<List<decimal>> Points { get; set; }
		public List<decimal> BBox { get; set; }
		public List<decimal> Centroid { get; set; }
		public List<List<decimal>> Multipolygons { get; set; }
	}
}
