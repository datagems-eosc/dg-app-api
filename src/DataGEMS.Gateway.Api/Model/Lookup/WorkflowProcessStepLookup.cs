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
	public class WorkflowProcessStepLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to specific process ids. If set, the list of ids must not be empty")]
		public List<Guid> ProcessIds { get; set; }

		public WorkflowProcessStepQuery Enrich(QueryFactory factory)
		{
			WorkflowProcessStepQuery query = factory.Query<WorkflowProcessStepQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ProcessIds != null) query.ProcessIds(this.ProcessIds);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);

			this.EnrichCommon(query);
			return query;
		}

		public class QueryValidator : BaseValidator<WorkflowProcessStepLookup>
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

			protected override IEnumerable<ISpecification> Specifications(WorkflowProcessStepLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowProcessStepLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowProcessStepLookup.Ids)]),
					//process ids must be null or not empty
					this.Spec()
						.Must(() => !item.ProcessIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowProcessStepLookup.ProcessIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowProcessStepLookup.ProcessIds)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowProcessStepLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowProcessStepLookup.ExcludedIds)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() =>  item.Order != null && !item.Order.IsEmpty)
						.FailOn(nameof(WorkflowProcessStepLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
