using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Enum;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class LanguagePilotLookup
	{
		[SwaggerSchema(description: "The query string to be used for the lookup")]
		public string Query { get; set; }
		[SwaggerSchema(description: "The list of dataset IDs to be used for the lookup")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "The list of linguistic features to be included in the lookup")]
		public List<LinguisticFeature> IncludedFeatures { get; set; }
		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }
		public Cite.Tools.FieldSet.FieldSet Project { get; set; }

		public class RequestValidator : BaseValidator<LanguagePilotLookup>
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

			protected override IEnumerable<ISpecification> Specifications(LanguagePilotLookup item)
			{
				return [
					//query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(LanguagePilotLookup.Query)).FailWith(this._localizer["validation_required", nameof(LanguagePilotLookup.Query)]),
					//dataset ids must not be empty if set
					this.Spec()
						.If(() => item.DatasetIds != null)
						.Must(() => item.DatasetIds.Count > 0)
						.FailOn(nameof(LanguagePilotLookup.DatasetIds)).FailWith(this._localizer["validation_required", nameof(LanguagePilotLookup.DatasetIds)]),
					//included features must be among the allowed set
					this.Spec()
						.If(() => item.IncludedFeatures != null)
						.Must(() => item.IncludedFeatures.All(x => Enum.IsDefined(typeof(LinguisticFeature), x)))
						.FailOn(nameof(LanguagePilotLookup.IncludedFeatures)).FailWith(this._localizer["validation_includedFeaturesMismatch"])
				];
			}
		}
	}
}
