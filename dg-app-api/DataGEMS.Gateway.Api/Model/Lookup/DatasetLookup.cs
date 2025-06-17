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
	public class DatasetLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description:"Limit lookup to items with specific ids. If set, the list of ids must not be empty", Title = "Dataset ids", Nullable = true )]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty", Title = "Excluded dataset ids", Nullable = true)]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to the specific collection ids. If set, the list of ids must not be empty", Title = "Collection ids", Nullable = true)]
		public List<Guid> CollectionIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose name matches the pattern", Title = "Name pattern", Nullable = true)]
		public String Like { get; set; }

		public DatasetLocalQuery Enrich(QueryFactory factory)
		{
			DatasetLocalQuery query = factory.Query<DatasetLocalQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.CollectionIds != null) query.CollectionIds(this.CollectionIds);
			if (!String.IsNullOrEmpty(this.Like)) query.Like(this.Like);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<DatasetLookup>
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

			protected override IEnumerable<ISpecification> Specifications(DatasetLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(DatasetLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(DatasetLookup.Ids)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(DatasetLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(DatasetLookup.ExcludedIds)]),
					//datasetIds must be null or not empty
					this.Spec()
						.Must(() => !item.CollectionIds.IsNotNullButEmpty())
						.FailOn(nameof(DatasetLookup.CollectionIds)).FailWith(this._localizer["validation_setButEmpty", nameof(DatasetLookup.CollectionIds)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(DatasetLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
