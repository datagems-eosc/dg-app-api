using Cite.Tools.Data.Query;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class CollectionLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items containing the specific dataset ids. If set, the list of ids must not be empty")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose name matches the pattern")]
		public String Like { get; set; }

		public CollectionLocalQuery Enrich(QueryFactory factory)
		{
			CollectionLocalQuery query = factory.Query<CollectionLocalQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.DatasetIds != null) query.DatasetIds(this.DatasetIds);
			if (!String.IsNullOrEmpty(this.Like)) query.Like(this.Like);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<CollectionLookup>
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

			protected override IEnumerable<ISpecification> Specifications(CollectionLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(CollectionLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(CollectionLookup.Ids)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(CollectionLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(CollectionLookup.ExcludedIds)]),
					//datasetIds must be null or not empty
					this.Spec()
						.Must(() => !item.DatasetIds.IsNotNullButEmpty())
						.FailOn(nameof(CollectionLookup.DatasetIds)).FailWith(this._localizer["validation_setButEmpty", nameof(CollectionLookup.DatasetIds)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(CollectionLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
