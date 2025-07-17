using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class ExplorationGeoQueryResponse
	{
		[JsonProperty("place")]
		public String Place { get; set; }

		[JsonProperty("most_relevant_wikidata")]
		public Dictionary<String, object> MostRelevantWikidata { get; set; }

		[JsonProperty("oql")]
		public OqlInfo Oql { get; set; }

		[JsonProperty("results")]
		public GeoQueryResults Results { get; set; }
	}

	public class OqlInfo
	{
		[JsonProperty("reasoning")]
		public String Reasoning { get; set; }

		[JsonProperty("OQL")]
		public String OqlText { get; set; }
	}

	public class GeoQueryResults
	{
		[JsonProperty("points")]
		public List<Point> Points { get; set; }

		[JsonProperty("geojson_data")]
		public Dictionary<String, object> GeoJsonData { get; set; }

		[JsonProperty("bounds")]
		public Bounds Bounds { get; set; }

		[JsonProperty("center")]
		public List<decimal> Center { get; set; }
	}

	public class Point
	{
		[JsonProperty("lon")]
		public decimal Lon { get; set; }

		[JsonProperty("lat")]
		public decimal Lat { get; set; }
	}

	public class Bounds
	{
		[JsonProperty("minlat")]
		public decimal MinLat { get; set; }

		[JsonProperty("minlon")]
		public decimal MinLon { get; set; }

		[JsonProperty("maxlat")]
		public decimal MaxLat { get; set; }

		[JsonProperty("maxlon")]
		public decimal MaxLon { get; set; }
	}
}
