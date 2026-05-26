using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class LanguagePilotRequest
	{
		public string Query { get; set; }
		public List<Guid> DatasetIds { get; set; }

		public class RequestValidator : BaseValidator<LanguagePilotRequest>
		{
			public RequestValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<RequestValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(LanguagePilotRequest item)
			{
				return [
					//query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(LanguagePilotRequest.Query)).FailWith(this._localizer["validation_required", nameof(LanguagePilotRequest.Query)]),
					//dataset ids must not be empty
					this.Spec()
						.If(() => item.DatasetIds == null || !item.DatasetIds.Any())
						.Must(() => false)
						.FailOn(nameof(LanguagePilotRequest.DatasetIds)).FailWith(this._localizer["validation_required", nameof(LanguagePilotRequest.DatasetIds)]),
				];
			}
		}
	}

	public class LanguagePilotResponse
	{
		public List<Metric> Features { get; set; }

		public class Metric : BaseMetric
		{
			public List<TermFrequency> TermFrequencies { get; set; }

			public class TermFrequency
			{
				public string Term { get; set; }
				public double Frequency { get; set; }
				public int Count { get; set; }
			}

			public MetricSentimentProfile SentimentProfile { get; set; }

			public class MetricSentimentProfile
			{
				public string Label { get; set; }
				public int PositiveTerms { get; set; }
				public int NegativeTerms { get; set; }
				public int NeutralTerms { get; set; }
				public int TotalTerms { get; set; }
				public decimal PolarityScore { get; set; }
				public decimal SubjectivityScore { get; set; }
			}

			public List<Collocation> Collocations { get; set; }
			public class Collocation
			{
				public List<string> Terms { get; set; }
				public int Count { get; set; }
				public double AssociationScore { get; set; }
			}
		}

		public List<BaseMetric> UsedChunks { get; set; }
		public class BaseMetric
		{
			public Guid DatasetId { get; set; }
			public string ObjectId { get; set; }
			public int Similarity { get; set; }
		}
	}
}
