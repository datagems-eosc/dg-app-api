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
	public class WorkflowProcessLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to specific user ids. If set, the list of ids must not be empty")]
		public List<Guid?> UserIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to specific dataset ids. If set, the list of ids must not be empty")]
		public List<Guid?> DatasetIds { get; set; }

		public WorkflowProcessQuery Enrich(QueryFactory factory)
		{
			WorkflowProcessQuery query = factory.Query<WorkflowProcessQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.UserIds != null) query.UserIds(this.UserIds);
			if (this.DatasetIds != null) query.DatasetIds(this.DatasetIds);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);

			this.EnrichCommon(query);
			return query;
		}

		public class QueryValidator : BaseValidator<WorkflowProcessLookup>
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

			protected override IEnumerable<ISpecification> Specifications(WorkflowProcessLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowProcessLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowProcessLookup.Ids)]),
					//user ids must be null or not empty
					this.Spec()
						.Must(() => !item.UserIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowProcessLookup.UserIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowProcessLookup.UserIds)]),
					//dataset ids must be null or not empty
					this.Spec()
						.Must(() => !item.DatasetIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowProcessLookup.DatasetIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowProcessLookup.DatasetIds)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowProcessLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowProcessLookup.ExcludedIds)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() =>  item.Order != null && !item.Order.IsEmpty)
						.FailOn(nameof(WorkflowProcessLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
