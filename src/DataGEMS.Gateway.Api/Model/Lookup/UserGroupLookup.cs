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
	public class UserGroupLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description:"Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<String> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<String> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items with specific semantics")]
		public List<string> Semantics { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose name matches the pattern")]
		public String Like { get; set; }

		public UserGroupHttpQuery Enrich(QueryFactory factory)
		{
			UserGroupHttpQuery query = factory.Query<UserGroupHttpQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.Semantics != null) query.Semantics(this.Semantics);
			if (!String.IsNullOrEmpty(this.Like)) query.Like(this.Like);

			query.Page = null;
			query.Order = null;

			return query;
		}

		public class QueryValidator : BaseValidator<UserGroupLookup>
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

			protected override IEnumerable<ISpecification> Specifications(UserGroupLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(UserGroupLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(UserGroupLookup.Ids)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(UserGroupLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(UserGroupLookup.ExcludedIds)]),
					//semantics must be null or not empty
					this.Spec()
						.Must(() => !item.Semantics.IsNotNullButEmpty())
						.FailOn(nameof(UserGroupLookup.Semantics)).FailWith(this._localizer["validation_setButEmpty", nameof(UserGroupLookup.Semantics)]),
					//paging not supported
					this.Spec()
						.Must(() => item.Page == null || item.Page.IsEmpty)
						.FailOn(nameof(UserGroupLookup.Page)).FailWith(this._localizer["validation_overPosting"]),
					//ordering not supported
					this.Spec()
						.Must(() => item.Order == null || item.Order.IsEmpty)
						.FailOn(nameof(UserGroupLookup.Order)).FailWith(this._localizer["validation_overPosting"]),
				};
			}
		}
	}
}
