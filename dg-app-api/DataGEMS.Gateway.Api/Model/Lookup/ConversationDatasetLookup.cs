using Cite.Tools.Data.Query;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class ConversationDatasetLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that have the specific conversation ids. If set, the list of ids must not be empty")]
		public List<Guid> ConversationIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that have the specific dataset ids. If set, the list of ids must not be empty")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are active, or inactive or both. If set, the list of flags must not be empty")]
		public List<IsActive> IsActive { get; set; }

		public ConversationDatasetQuery Enrich(QueryFactory factory)
		{
			ConversationDatasetQuery query = factory.Query<ConversationDatasetQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.ConversationIds != null) query.ConversationIds(this.ConversationIds);
			if (this.DatasetIds != null) query.DatasetIds(this.DatasetIds);
			if (this.IsActive != null) query.IsActive(this.IsActive);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<ConversationDatasetLookup>
		{
			public QueryValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<QueryValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(ConversationDatasetLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(ConversationDatasetLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationDatasetLookup.Ids)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(ConversationDatasetLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationDatasetLookup.ExcludedIds)]),
					//conversationIds must be null or not empty
					this.Spec()
						.Must(() => !item.ConversationIds.IsNotNullButEmpty())
						.FailOn(nameof(ConversationDatasetLookup.ConversationIds)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationDatasetLookup.ConversationIds)]),
					//datasetIds must be null or not empty
					this.Spec()
						.Must(() => !item.DatasetIds.IsNotNullButEmpty())
						.FailOn(nameof(ConversationDatasetLookup.DatasetIds)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationDatasetLookup.DatasetIds)]),
					//isactive must be null or not empty
					this.Spec()
						.Must(() => !item.IsActive.IsNotNullButEmpty())
						.FailOn(nameof(ConversationDatasetLookup.IsActive)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationDatasetLookup.IsActive)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(ConversationDatasetLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
