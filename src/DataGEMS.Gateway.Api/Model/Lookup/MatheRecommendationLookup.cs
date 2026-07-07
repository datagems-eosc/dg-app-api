using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class MatheRecommendationLookup
	{
		[SwaggerSchema(description: "The question ID to be used for the lookup")]
		public string QuestionId { get; set; }
		[SwaggerSchema(description: "The question to be used for the lookup")]
		public string Question { get; set; }
		[SwaggerSchema(description: "The number of recommended materials to be returned")]
		public int RecommendedMaterialsCount { get; set; }
		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }
		public Cite.Tools.FieldSet.FieldSet Project { get; set; }

		public class RequestValidator : BaseValidator<MatheRecommendationLookup>
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

			protected override IEnumerable<ISpecification> Specifications(MatheRecommendationLookup item)
			{
				return [
					// question id must be set and not empty
					this.Spec()
						.Must(() => this.IsNotEmpty(item.QuestionId))
						.FailOn(nameof(MatheRecommendationLookup.QuestionId)).FailWith(this._localizer["validation_required", nameof(MatheRecommendationLookup.QuestionId)]),
					// question must be set and not empty
					this.Spec()
						.Must(() => this.IsNotEmpty(item.Question))
						.FailOn(nameof(MatheRecommendationLookup.Question)).FailWith(this._localizer["validation_required", nameof(MatheRecommendationLookup.Question)]),
					// recommended materials count must be greater than 0
					this.Spec()
						.Must(() => item.RecommendedMaterialsCount > 0)
						.FailOn(nameof(MatheRecommendationLookup.RecommendedMaterialsCount)).FailWith(this._localizer["validation_required", nameof(MatheRecommendationLookup.RecommendedMaterialsCount)]),
					//conversation options must be valid if set
					this.RefSpec()
						.If(() => item.ConversationOptions != null)
						.On(nameof(MatheRecommendationLookup.ConversationOptions))
						.Over(item.ConversationOptions)
						.Using(()=>_validatorFactory[typeof(ConversationOptions.ConversationOptionsValidator)]),
				];
			}
		}
	}
}
