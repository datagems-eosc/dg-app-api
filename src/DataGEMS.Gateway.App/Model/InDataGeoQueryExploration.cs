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
		public static string ModelVersion = "V1";


		public String Place { get; set; }

		public Dictionary<String, object> MostRelevantWikidata { get; set; }

		public OqlInfo Oql { get; set; }

		public GeoQueryResults Results { get; set; }
	}

	public class OqlInfo
	{
		public String Reasoning { get; set; }
		public String OqlText { get; set; }
	}

	public class GeoQueryResults
	{
		public List<Point> Points { get; set; }

		public Dictionary<String, object> GeoJsonData { get; set; }

		public Bounds Bounds { get; set; }

		public List<decimal> Center { get; set; }
	}

	public class Point
	{
		public decimal Lon { get; set; }
		public decimal Lat { get; set; }
	}

	public class Bounds
	{
		public decimal MinLat { get; set; }
		public decimal MinLon { get; set; }
		public decimal MaxLat { get; set; }
		public decimal MaxLon { get; set; }
	}
}
