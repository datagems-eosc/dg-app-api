using DataGEMS.Gateway.App.Common.Enum;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Model
{
	public class LanguagePilotRequest
	{
		public string Query { get; set; }
		public List<Guid> DatasetIds { get; set; }
		public List<LinguisticFeature> IncludedFeatures { get; set; }

		//GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
	}

	public class LanguagePilotResponse
	{
		[JsonProperty("rag_output")]
		public Service.Discovery.Model.CorpusAnalysisResponse RagOutput { get; set; }

		[JsonProperty("features")]
		public List<Metric> Features { get; set; }

		public class Metric : BaseMetric
		{
			[JsonProperty("term_frequency")]
			public List<TermFrequency> TermFrequencies { get; set; }

			public class TermFrequency
			{
				[JsonProperty("term")]
				public string Term { get; set; }
				[JsonProperty("frequency")]
				public double Frequency { get; set; }
				[JsonProperty("count")]
				public int Count { get; set; }
			}

			[JsonProperty("sentiment_profile")]
			public MetricSentimentProfile SentimentProfile { get; set; }

			public class MetricSentimentProfile
			{
				[JsonProperty("label")]
				public string Label { get; set; }
				[JsonProperty("positive_terms")]
				public int PositiveTerms { get; set; }
				[JsonProperty("negative_terms")]
				public int NegativeTerms { get; set; }
				[JsonProperty("neutral_terms")]
				public int NeutralTerms { get; set; }
				[JsonProperty("total_terms")]
				public int TotalTerms { get; set; }
				[JsonProperty("polarity_score")]
				public double PolarityScore { get; set; }
				[JsonProperty("subjectivity_score")]
				public double SubjectivityScore { get; set; }
			}

			[JsonProperty("collocations")]
			public List<Collocation> Collocations { get; set; }
			public class Collocation
			{
				[JsonProperty("terms")]
				public List<string> Terms { get; set; }
				[JsonProperty("count")]
				public int Count { get; set; }
				[JsonProperty("association_score")]
				public double AssociationScore { get; set; }
			}
		}

		[JsonProperty("used_chunks")]
		public List<BaseMetric> UsedChunks { get; set; }
		public class BaseMetric
		{
			[JsonProperty("dataset_id")]
			public Guid DatasetId { get; set; }
			[JsonProperty("object_id")]
			public string ObjectId { get; set; }
			[JsonProperty("similarity")]
			public double Similarity { get; set; }
		}
	}
}
