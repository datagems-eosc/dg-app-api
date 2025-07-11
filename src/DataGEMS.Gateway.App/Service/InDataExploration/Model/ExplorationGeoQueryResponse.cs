using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class ExplorationGeoQueryResponse
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
	}
}
