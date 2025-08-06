using Cite.Tools.Validation;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.App.Common;
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
		[SwaggerSchema(description: "Limit lookup to datasets from the specific ids. If set, the list of ids must not be empty")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to datasets from the specific collections Ids. If set, the list of ids must not be empty")]
		public List<Guid> CollectionIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to datasets from the specific user collections Ids. If set, the list of ids must not be empty")]
		public List<Guid> UserCollectionIds { get; set; }
		[SwaggerSchema(description: "The conversation handling options")]
		public ConversationOptions ConversationOptions { get; set; }
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
					//conversation options must be valid if set
					this.RefSpec()
						.If(() => item.ConversationOptions != null)
						.On(nameof(CrossDatasetDiscoveryLookup.ConversationOptions))
						.Over(item.ConversationOptions)
						.Using(()=>_validatorFactory[typeof(ConversationOptions.ConversationOptionsValidator)]),
					//datasetIds must be null or not empty
					this.Spec()
						.Must(() => !item.DatasetIds.IsNotNullButEmpty())
						.FailOn(nameof(CrossDatasetDiscoveryLookup.DatasetIds)).FailWith(this._localizer["validation_setButEmpty", nameof(CrossDatasetDiscoveryLookup.DatasetIds)]),
					//collectionIds must be null or not empty
					this.Spec()
						.Must(() => !item.CollectionIds.IsNotNullButEmpty())
						.FailOn(nameof(CrossDatasetDiscoveryLookup.CollectionIds)).FailWith(this._localizer["validation_setButEmpty", nameof(CrossDatasetDiscoveryLookup.CollectionIds)]),
					//user collectionIds must be null or not empty
					this.Spec()
						.Must(() => !item.UserCollectionIds.IsNotNullButEmpty())
						.FailOn(nameof(CrossDatasetDiscoveryLookup.UserCollectionIds)).FailWith(this._localizer["validation_setButEmpty", nameof(CrossDatasetDiscoveryLookup.UserCollectionIds)]),
				};
			}
		}
	}
}
