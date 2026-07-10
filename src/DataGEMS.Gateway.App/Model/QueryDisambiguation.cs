using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Model
{
	public class QueryDisambiguation
	{
		public static string ModelVersion = "V1";
		public List<string> Results { get; set; }
		public QueryDisambiguationMetadata Metadata { get; set; }
	}

	public class QueryDisambiguationMetadata
	{
		[JsonProperty("provider")]
		public string Provider { get; set; }
		[JsonProperty("reference_datetime")]
		public DateTime ReferenceDateTime { get; set; }
		[JsonProperty("query")]
		public string Query { get; set; }
		[JsonProperty("result")]
		public MetadataResult Result { get; set; }

		public class MetadataResult 
		{
			[JsonProperty("general_notes")]
			public List<GeneralNote> GeneralNotes { get; set; }
			[JsonProperty("ambiguous_parts")]
			public List<AmbiguousPart> AmbiguousParts { get; set; }
			[JsonProperty("overall_clarity")]
			public string OverallClarity { get; set; }

			public class GeneralNote
			{
				[JsonProperty("aspect")]
				public string Aspect { get; set; }
				[JsonProperty("description")]
				public string Description { get; set; }
				[JsonProperty("severity")]
				public string Severity { get; set; }
			}

			public class AmbiguousPart
			{
				[JsonProperty("original_text")]
				public string OriginalText { get; set; }
				[JsonProperty("ambiguity_type")]
				public string AmbiguityType { get; set; }
				[JsonProperty("reason")]
				public string Reason { get; set; }
				[JsonProperty("rephrased_options")]
				public List<string> RephrasedOptions { get; set; }
			}
		}
	}
}
