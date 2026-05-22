using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class MatheRecommendationRequest
	{
		public string QuestionId { get; set; }
		public string Question { get; set; }
		public int RecommendedMaterialsCount { get; set; }

		public class RequestValidator : BaseValidator<MatheRecommendationRequest>
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

			protected override IEnumerable<ISpecification> Specifications(MatheRecommendationRequest item)
			{
				return [
					// question id must be set and not empty
					this.Spec()
						.Must(() => this.IsNotEmpty(item.QuestionId))
						.FailOn(nameof(MatheRecommendationRequest.QuestionId)).FailWith(this._localizer["validation_required", nameof(MatheRecommendationRequest.QuestionId)]),
					// question must be set and not empty
					this.Spec()
						.Must(() => this.IsNotEmpty(item.Question))
						.FailOn(nameof(MatheRecommendationRequest.Question)).FailWith(this._localizer["validation_required", nameof(MatheRecommendationRequest.Question)]),
					// recommended materials count must be greater than 0
					this.Spec()
						.Must(() => item.RecommendedMaterialsCount > 0)
						.FailOn(nameof(MatheRecommendationRequest.RecommendedMaterialsCount)).FailWith(this._localizer["validation_required", nameof(MatheRecommendationRequest.RecommendedMaterialsCount)]),
				];
			}
		}
	}
}
