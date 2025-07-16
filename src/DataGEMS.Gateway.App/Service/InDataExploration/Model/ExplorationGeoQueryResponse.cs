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


	/*public class ExplorationGeoQueryResponse
	{
		[JsonProperty("place")]
		public string Place { get; set; }

		[JsonProperty("most_relevant_wikidata")]
		public WikidataInfo MostRelevantWikidata { get; set; }

		[JsonProperty("oql")]
		public string Oql { get; set; }

		[JsonProperty("results")]
		public GeoQueryResults Results { get; set; }
	}

	public class WikidataInfo
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }
	}

	public class GeoQueryResults
	{
		[JsonProperty("points")]
		public List<List<decimal>> Points { get; set; }

		[JsonProperty("bbox")]
		public List<decimal> BBox { get; set; }

		[JsonProperty("centroid")]
		public List<decimal> Centroid { get; set; }

		[JsonProperty("multipolygons")]
		public List<List<decimal>> Multipolygons { get; set; }		// TODO: What does it return exactly???
	}*/
}
