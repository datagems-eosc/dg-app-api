using Cite.Tools.Validation;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Service.InDataExploration;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;


namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class InDataExplorationSqlLookup
	{
		[SwaggerSchema(description: "The user query in natural language. Cannot be empty.")]      //TODO: Swagger info
		public string Query { get; set; }

		[SwaggerSchema(description: "An object containing additional parameters that will be passed as-is to the underlying service.")] //TODO: Swagger info
		public Dictionary<string,object> Parameters { get; set; }

		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }

		public Cite.Tools.FieldSet.FieldSet Project { get; set; }


		public class InDataExplorationTextToSqlLookupValidator : BaseValidator<InDataExplorationSqlLookup>
		{
			public InDataExplorationTextToSqlLookupValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<InDataExplorationTextToSqlLookupValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;


			protected override IEnumerable<ISpecification> Specifications(InDataExplorationSqlLookup item)
			{
				return new ISpecification[] {
					// Query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(InDataExplorationSqlLookup.Query))
						.FailWith(this._localizer["validation_required", nameof(InDataExplorationSqlLookup.Query)])
					//TODO: What about Parameters???
				};
			}
		}


		public class SqlQueryParametersInfo
		{
			public SqlQueryResultsInfo Results { get; set; }
		}

		public class SqlQueryResultsInfo
		{
			public List<List<decimal>> Points { get; set; }
		}
	}
}
