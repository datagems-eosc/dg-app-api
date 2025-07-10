using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model
{
	public class CrossDatasetDiscoveryLookup
	{
		[SwaggerSchema(description: "The user query for which datasets must be discovered")]
		public String Query { get; set; }
		[SwaggerSchema(description: "The number of results to retrieve for the query")]
		public int? ResultCount { get; set; }
		[SwaggerSchema(description: "The conversation id to include the user query")]
		public Guid? ConversationId { get; set; }
		[SwaggerSchema(description: "Option to auto create new conversation if no conversation id provided. Leaving it empty is equivalent to false. Should not be true if Conversation id is provided")]
		public Boolean? AutoCreateConversation { get; set; }

		public Cite.Tools.FieldSet.FieldSet Project { get; set; }

		public class CrossDatasetDiscoveryLookupValidator : BaseValidator<CrossDatasetDiscoveryLookup>
		{
			public CrossDatasetDiscoveryLookupValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<CrossDatasetDiscoveryLookupValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(CrossDatasetDiscoveryLookup item)
			{
				return new ISpecification[] { 
					// Query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(CrossDatasetDiscoveryLookup.Query))
						.FailWith(this._localizer["validation_required", nameof(CrossDatasetDiscoveryLookup.Query)]),
					// If ResultCount is provided, must be > 0
					this.Spec()
						.If(() => item.ResultCount.HasValue)
						.Must(() => item.ResultCount.Value > 0)
						.FailOn(nameof(CrossDatasetDiscoveryLookup.ResultCount))
						.FailWith(this._localizer["validation_invalidValue", nameof(CrossDatasetDiscoveryLookup.ResultCount)]),
					// If conversation specified, autocreate cannot be true
					this.Spec()
						.If(() => item.ConversationId.HasValue)
						.Must(() => !item.AutoCreateConversation.HasValue || (item.AutoCreateConversation.HasValue && !item.AutoCreateConversation.Value))
						.FailOn(nameof(CrossDatasetDiscoveryLookup.AutoCreateConversation))
						.FailWith(this._localizer["validation_invalidValue", nameof(CrossDatasetDiscoveryLookup.AutoCreateConversation)]),
				};
			}
		}
	}
}
