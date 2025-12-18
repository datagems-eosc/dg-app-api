using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class QueryRecommendationLookup
	{
		[SwaggerSchema(description: "The query the recommendation will be based on")]
		public string Query { get; set; }
		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }
		public Cite.Tools.FieldSet.FieldSet Project { get; set; }


		public class QueryRecommendationLookupValidator : BaseValidator<QueryRecommendationLookup>
		{
			public QueryRecommendationLookupValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<QueryRecommendationLookupValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(QueryRecommendationLookup item)
			{
				return [ 
					// Query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(QueryRecommendationLookup.Query))
						.FailWith(this._localizer["validation_required", nameof(QueryRecommendationLookup.Query)]),
					//conversation options must be valid if set
					this.RefSpec()
						.If(() => item.ConversationOptions != null)
						.On(nameof(QueryRecommendationLookup.ConversationOptions))
						.Over(item.ConversationOptions)
						.Using(()=>_validatorFactory[typeof(ConversationOptions.ConversationOptionsValidator)]),
				];
			}
		}
	}
}
