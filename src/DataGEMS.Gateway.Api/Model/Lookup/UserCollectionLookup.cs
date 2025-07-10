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
	public class UserCollectionLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to specific user ids. If set, the list of ids must not be empty")]
		public List<Guid> UserIds { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are active, or inactive or both. If set, the list of flags must not be empty")]
		public List<IsActive> IsActive { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose name or email matches the pattern")]
		public String Like { get; set; }

		public UserCollectionQuery Enrich(QueryFactory factory)
		{
			UserCollectionQuery query = factory.Query<UserCollectionQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.UserIds != null) query.UserIds(this.UserIds);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.IsActive != null) query.IsActive(this.IsActive);
			if (!String.IsNullOrEmpty(this.Like)) query.Like(this.Like);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<UserCollectionLookup>
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

			protected override IEnumerable<ISpecification> Specifications(UserCollectionLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(UserCollectionLookup.Ids)]),
					//user ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.UserIds)).FailWith(this._localizer["validation_setButEmpty", nameof(UserCollectionLookup.UserIds)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(UserCollectionLookup.ExcludedIds)]),
					//datasetIds must be null or not empty
					this.Spec()
						.Must(() => !item.IsActive.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.IsActive)).FailWith(this._localizer["validation_setButEmpty", nameof(UserCollectionLookup.IsActive)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(UserCollectionLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}

		public class QueryMeValidator : BaseValidator<UserCollectionLookup>
		{
			public QueryMeValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<QueryMeValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(UserCollectionLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(UserCollectionLookup.Ids)]),
					//user ids must not be set
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.UserIds)).FailWith(this._localizer["validation_overPosting"]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(UserCollectionLookup.ExcludedIds)]),
					//datasetIds must be null or not empty
					this.Spec()
						.Must(() => !item.IsActive.IsNotNullButEmpty())
						.FailOn(nameof(UserCollectionLookup.IsActive)).FailWith(this._localizer["validation_setButEmpty", nameof(UserCollectionLookup.IsActive)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(UserCollectionLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
