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

		[SwaggerSchema(description: "Limit lookup to items whose run start is in the specific period")]
		public RangeOf<DateOnly?> StartDate { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose run end is in the specific period")]
		public RangeOf<DateOnly?> EndDate { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that run after this specific period")]
		public RangeOf<DateOnly?> RunAfter { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are queued at this specific period")]
		public RangeOf<DateOnly?> QueuedAt { get; set; }
		[SwaggerSchema(description: "Limit lookup to items who are at a specific state. If set, the list of values must not be empty")]
		public List<WorkflowRunState> State { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are specifically triggered by that exact state")]
		public String? TriggeredBy { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are specifically triggered by that exact run type")]
		public List<WorkflowRunType> RunType { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Dag Id List")]
		public List<String> ListDagIds { get; set; }
		
		public WorkflowExecutionQuery Enrich(QueryFactory factory)
		{
			WorkflowExecutionQuery query = factory.Query<WorkflowExecutionQuery>();

			if (this.RunAfter != null) query.RunAfter(this.RunAfter);
			if (this.QueuedAt != null) query.QueuedAt(this.QueuedAt);
			if (this.StartDate != null) query.StartDate(this.StartDate);
			if (this.EndDate != null) query.EndDate(this.EndDate);
			if (this.State != null) query.State(this.State);
			if (this.RunType != null) query.RunType(this.RunType);
			if (this.ListDagIds != null) query.ListDagIds(this.ListDagIds);
			if (!String.IsNullOrEmpty(this.TriggeredBy))query.TriggeredBy(this.TriggeredBy);

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
					//Ids must be null or not empty
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
