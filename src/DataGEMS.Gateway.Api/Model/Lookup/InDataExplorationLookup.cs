using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class InDataExplorationLookup
	{
		[SwaggerSchema(description: "A string containing the user query in natural language. Cannot be empty.")]
		public String Query { get; set; }
		[SwaggerSchema(description: "The datasets to look into. Cannot be empty.")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }
		public Cite.Tools.FieldSet.FieldSet Project { get; set; }


		public class InDataExplorationLookupValidator : BaseValidator<InDataExplorationLookup>
		{
			public InDataExplorationLookupValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<InDataExplorationLookupValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(InDataExplorationLookup item)
			{
				return new ISpecification[] { 
					// Query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(InDataExplorationLookup.Query))
						.FailWith(this._localizer["validation_required", nameof(InDataExplorationLookup.Query)]),
					// Datasets must always be set
					this.Spec()
						.Must(() => item.DatasetIds != null && item.DatasetIds.Count > 0)
						.FailOn(nameof(InDataExplorationLookup.DatasetIds))
						.FailWith(this._localizer["validation_required", nameof(InDataExplorationLookup.DatasetIds)]),
				};
			}
		}
	}
}
