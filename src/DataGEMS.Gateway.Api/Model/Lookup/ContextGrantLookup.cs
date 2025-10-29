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
	public class ContextGrantLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items target specific dataset ids. If set, the list of ids must not be empty")]
		public List<Guid> DatasetIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items target specific collection ids. If set, the list of ids must not be empty")]
		public List<Guid> CollectionIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items granting specific roles. If set, the list of roles must not be empty")]
		public List<String> Roles { get; set; }
		[SwaggerSchema(description: "Limit lookup to items granting access to specific subject. If empty, use current user")]
		public String SubjectId { get; set; }

		public ContextGrantQuery Enrich(QueryFactory factory)
		{
			ContextGrantQuery query = factory.Query<ContextGrantQuery>();

			if (this.DatasetIds != null) query.DatasetIds(this.DatasetIds);
			if (this.CollectionIds != null) query.CollectionIds(this.CollectionIds);
			if (this.Roles != null) query.Roles(this.Roles);
			if (!String.IsNullOrEmpty(this.SubjectId)) query.Subject(this.SubjectId);

			this.EnrichCommon(query);

			this.Page = null;
			this.Order = null;

			return query;
		}

		public class QueryValidator : BaseValidator<ContextGrantLookup>
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

			protected override IEnumerable<ISpecification> Specifications(ContextGrantLookup item)
			{
				return new ISpecification[]{
					//dataset ids must be null or not empty
					this.Spec()
						.Must(() => !item.DatasetIds.IsNotNullButEmpty())
						.FailOn(nameof(ContextGrantLookup.DatasetIds)).FailWith(this._localizer["validation_setButEmpty", nameof(ContextGrantLookup.DatasetIds)]),
					//collection ids must be null or not empty
					this.Spec()
						.Must(() => !item.CollectionIds.IsNotNullButEmpty())
						.FailOn(nameof(ContextGrantLookup.CollectionIds)).FailWith(this._localizer["validation_setButEmpty", nameof(ContextGrantLookup.CollectionIds)]),
					//roles must be null or not empty
					this.Spec()
						.Must(() => !item.Roles.IsNotNullButEmpty())
						.FailOn(nameof(ContextGrantLookup.Roles)).FailWith(this._localizer["validation_setButEmpty", nameof(ContextGrantLookup.Roles)]),
					//ordering must not be set
					this.Spec()
						.Must(() => item.Order == null || item.Order.IsEmpty)
						.FailOn(nameof(ContextGrantLookup.Order)).FailWith(this._localizer["validation_overPosting"]),
					//paging must not be set
					this.Spec()
						.Must(() => item.Page == null || item.Page.IsEmpty)
						.FailOn(nameof(ContextGrantLookup.Page)).FailWith(this._localizer["validation_overPosting"]),
				};
			}
		}
	}
}
