using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class QueryDisambiguationLookup
	{
		[SwaggerSchema(description: "The query the disambiguation will be based on")]
		public string Query { get; set; }
		[SwaggerSchema(description: "The datasets the disambiguation will be based on")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }
		public Cite.Tools.FieldSet.FieldSet Project { get; set; }


		public class QueryDisambiguationLookupValidator : BaseValidator<QueryDisambiguationLookup>
		{
			public QueryDisambiguationLookupValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<QueryDisambiguationLookupValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(QueryDisambiguationLookup item)
			{
				return [ 
					// Query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(QueryDisambiguationLookup.Query))
						.FailWith(this._localizer["validation_required", nameof(QueryDisambiguationLookup.Query)]),
					// DatasetIds must always be set
					this.Spec()
						.Must(() => item.DatasetIds != null && item.DatasetIds.Count > 0)
						.FailOn(nameof(QueryDisambiguationLookup.DatasetIds))
						.FailWith(this._localizer["validation_required", nameof(QueryDisambiguationLookup.DatasetIds)]),
					//conversation options must be valid if set
					this.RefSpec()
						.If(() => item.ConversationOptions != null)
						.On(nameof(QueryDisambiguationLookup.ConversationOptions))
						.Over(item.ConversationOptions)
						.Using(()=>_validatorFactory[typeof(ConversationOptions.ConversationOptionsValidator)]),
				];
			}
		}
	}
}
