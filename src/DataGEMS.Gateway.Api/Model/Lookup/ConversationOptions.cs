using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class ConversationOptions
	{
		[SwaggerSchema(description: "The conversation id to include the user query")]
		public Guid? ConversationId { get; set; }
		[SwaggerSchema(description: "Option to auto create new conversation if no conversation id provided. Leaving it empty is equivalent to false. Should not be true if Conversation id is provided")]
		public Boolean? AutoCreateConversation { get; set; }
		[SwaggerSchema(description: "Option to auto update the conversation datasets with the query context. Leaving it empty is equivalent to false. Should not be true if Conversation id is not provided of autocreate is set to true")]
		public Boolean? AutoUpdateDatasets { get; set; }

		public class ConversationOptionsValidator : BaseValidator<ConversationOptions>
		{
			public ConversationOptionsValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<ConversationOptionsValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(ConversationOptions item)
			{
				return new ISpecification[] { 
					// If conversation specified, autocreate cannot be true
					this.Spec()
						.If(() => item.ConversationId.HasValue)
						.Must(() => !item.AutoCreateConversation.HasValue || (item.AutoCreateConversation.HasValue && !item.AutoCreateConversation.Value))
						.FailOn(nameof(ConversationOptions.AutoCreateConversation))
						.FailWith(this._localizer["validation_invalidValue", nameof(ConversationOptions.AutoCreateConversation)]),
					// If auto update datasets set to true, conversation or auto creation must be set
					this.Spec()
						.If(() => item.AutoUpdateDatasets.HasValue && item.AutoUpdateDatasets.Value)
						.Must(() => item.ConversationId.HasValue || (item.AutoCreateConversation.HasValue && item.AutoCreateConversation.Value))
						.FailOn(nameof(ConversationOptions.AutoUpdateDatasets))
						.FailWith(this._localizer["validation_invalidValue", nameof(ConversationOptions.AutoUpdateDatasets)]),
				};
			}
		}
	}
}
