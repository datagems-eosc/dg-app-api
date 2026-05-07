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
	public class AdHocQueryLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to specific user ids. If set, the list of ids must not be empty")]
		public List<Guid> UserIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to specific dataset ids. If set, the list of ids must not be empty")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are active, or inactive or both. If set, the list of flags must not be empty")]
		public List<IsActive> IsActive { get; set; }

		public AdHocQueryQuery Enrich(QueryFactory factory)
		{
			AdHocQueryQuery query = factory.Query<AdHocQueryQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.UserIds != null) query.UserIds(this.UserIds);
			if (this.DatasetIds != null) query.UserIds(this.DatasetIds);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.IsActive != null) query.IsActive(this.IsActive);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<AdHocQueryLookup>
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

			protected override IEnumerable<ISpecification> Specifications(AdHocQueryLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(AdHocQueryLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(AdHocQueryLookup.Ids)]),
					//user ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(AdHocQueryLookup.UserIds)).FailWith(this._localizer["validation_setButEmpty", nameof(AdHocQueryLookup.UserIds)]),
					//dataset ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(AdHocQueryLookup.DatasetIds)).FailWith(this._localizer["validation_setButEmpty", nameof(AdHocQueryLookup.DatasetIds)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(AdHocQueryLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(AdHocQueryLookup.ExcludedIds)]),
					//statuses must be null or not empty
					this.Spec()
						.Must(() => !item.IsActive.IsNotNullButEmpty())
						.FailOn(nameof(AdHocQueryLookup.IsActive)).FailWith(this._localizer["validation_setButEmpty", nameof(AdHocQueryLookup.IsActive)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() =>  item.Order != null && !item.Order.IsEmpty)
						.FailOn(nameof(AdHocQueryLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
