using Cite.Tools.Validation;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model
{
	public class IntentDisambiguationRequest
	{
		[SwaggerSchema(description: "The query to disambiguate")]
		public string Query { get; set; }
		[SwaggerSchema(description: "The context of the request, including domain, dataset IDs, and history")]
		public IntentDisambiguationRequestContext Context { get; set; }
		[SwaggerSchema(description: "Options for the conversation, including conversation ID")]
		public ConversationOptions ConversationOptions { get; set; }

		public class IntentDisambiguationRequestContext
		{
			[SwaggerSchema(description: "The domain of the request")]
			public string Domain { get; set; }
			[SwaggerSchema(description: "The IDs of the datasets involved")]
			public List<Guid> DatasetIds { get; set; }
			[SwaggerSchema(description: "The history of the conversation")]
			public List<string> History { get; set; }
		}

		public class Validator : BaseValidator<IntentDisambiguationRequest>
		{
			public Validator(
				IStringLocalizer<Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<Validator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(IntentDisambiguationRequest item)
			{
				return [
					//query is required
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(IntentDisambiguationRequest.Query)).FailWith(this._localizer["validation_required", nameof(IntentDisambiguationRequest.Query)]),
					//query must be at most 2000 characters
					this.Spec()
						.Must(() => item.Query.Length >= 1 && item.Query.Length <= 2000)
						.FailOn(nameof(IntentDisambiguationRequest.Query)).FailWith(this._localizer["validation_maxLength", nameof(IntentDisambiguationRequest.Query)]),
					//query must be at least 1 character
					this.Spec()
						.Must(() => item.Query.Length >= 1 && item.Query.Length <= 2000)
						.FailOn(nameof(IntentDisambiguationRequest.Query)).FailWith(this._localizer["validation_minLength", nameof(IntentDisambiguationRequest.Query)]),
					//if history is set, it must be at most 5 entries
					this.Spec()
						.Must(() => item.Context == null || item.Context.History == null || item.Context.History.Count <= 5)
						.FailOn(nameof(IntentDisambiguationRequest.Context.History)).FailWith(this._localizer["validation_maxLength", nameof(IntentDisambiguationRequest.Context.History)]),
				];
			}
		}
	}
}
