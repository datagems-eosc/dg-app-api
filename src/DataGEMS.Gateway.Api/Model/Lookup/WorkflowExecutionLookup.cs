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
	public class WorkflowExecutionLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items that are in the Workflow Id List")]
		public List<String> WorkflowIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose logical date is in the specific period")]
		public RangeOf<DateOnly?> LogicalDateRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose run start is in the specific period")]
		public RangeOf<DateOnly?> StartDateRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose run end is in the specific period")]
		public RangeOf<DateOnly?> EndDateRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that run after this specific period")]
		public RangeOf<DateOnly?> RunAfterRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are specifically triggered by that exact run type")]
		public List<WorkflowRunType> RunType { get; set; }
		[SwaggerSchema(description: "Limit lookup to items who are at a specific state. If set, the list of values must not be empty")]
		public List<WorkflowRunState> State { get; set; }
		
		public WorkflowExecutionHttpQuery Enrich(QueryFactory factory)
		{
			WorkflowExecutionHttpQuery query = factory.Query<WorkflowExecutionHttpQuery>();

			if (this.WorkflowIds != null) query.WorkflowIds(this.WorkflowIds);
			if (this.LogicalDateRange != null) query.LogicalDateRange(this.LogicalDateRange);
			if (this.StartDateRange != null) query.StartDateRange(this.StartDateRange);
			if (this.EndDateRange != null) query.EndDateRange(this.EndDateRange);
			if (this.RunAfterRange != null) query.RunAfterRange(this.RunAfterRange);
			if (this.RunType != null) query.RunType(this.RunType);
			if (this.State != null) query.State(this.State);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<WorkflowExecutionLookup>
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

			protected override IEnumerable<ISpecification> Specifications(WorkflowExecutionLookup item)
			{
				return new ISpecification[]{
					//Workflow Ids must be null or not empty
					this.Spec()
						.Must(() => !item.WorkflowIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowExecutionLookup.WorkflowIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowExecutionLookup.WorkflowIds)]),
					//Run Type must be null or not empty
					this.Spec()
						.Must(() => !item.RunType.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowExecutionLookup.RunType)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowExecutionLookup.RunType)]),
					//states must be null or not empty
					this.Spec()
						.Must(() => !item.State.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowExecutionLookup.State)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowExecutionLookup.State)]),
					//Paging with Ordering is only supported !
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(WorkflowExecutionLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}

		}
	}
}
