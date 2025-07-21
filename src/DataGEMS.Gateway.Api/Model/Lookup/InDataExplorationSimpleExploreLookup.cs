using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class InDataExplorationSimpleExploreLookup
	{
		[SwaggerSchema(description: "A string containing the user query in natural language. Cannot be empty.")]    //TODO: Do we need to change the description???
		public String Query { get; set; }
		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }
		public Cite.Tools.FieldSet.FieldSet Project { get; set; }


		public class InDataExplorationSimpleExploreLookupValidator : BaseValidator<InDataExplorationSimpleExploreLookup>
		{
			public InDataExplorationSimpleExploreLookupValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<InDataExplorationSimpleExploreLookupValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(InDataExplorationSimpleExploreLookup item)
			{
				return new ISpecification[] { 
					// Query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(InDataExplorationSimpleExploreLookup.Query))
						.FailWith(this._localizer["validation_required", nameof(InDataExplorationSimpleExploreLookup.Query)]),
				};
			}
		}
	}
}
