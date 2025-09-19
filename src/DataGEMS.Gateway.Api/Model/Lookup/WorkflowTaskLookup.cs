using Cite.Tools.Data.Query;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class WorkflowTaskLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items that are in the Task Id List")]
		public List<String> TaskIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Workflow Id List")]
		public List<String> WorkflowIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Workflow Run Id List")]
		public List<String> WorkflowRunIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Pool List")]
		public List<String> Pool { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Queue states List")]
		public List<String> Queue { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Workflow Id List")]
		public List<String> Executor { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose duration is in the specific period")]
		public RangeOf<Decimal?> DurationRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose logical date range is in the specific period")]
		public RangeOf<DateOnly?> LogicalDateRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose run start is in the specific period")]
		public RangeOf<DateOnly?> StartDateRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose run end is in the specific period")]
		public RangeOf<DateOnly?> EndDateRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that run after this specific period")]
		public RangeOf<DateOnly?> RunAfterRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items who are at a specific state. If set, the list of values must not be empty")]
		public List<WorkflowRunState> State { get; set; }

		public WorkflowTaskHttpQuery Enrich(QueryFactory factory)
		{
			WorkflowTaskHttpQuery query = factory.Query<WorkflowTaskHttpQuery>();

			if (this.WorkflowIds != null) query.WorkflowIds(this.WorkflowIds);
			if (this.DurationRange != null) query.DurationRange(this.DurationRange);
			if (this.LogicalDateRange != null) query.LogicalDateRange(this.LogicalDateRange);
			if (this.StartDateRange != null) query.StartDateRange(this.StartDateRange);
			if (this.EndDateRange != null) query.EndDateRange(this.EndDateRange);
			if (this.RunAfterRange != null) query.RunAfterRange(this.RunAfterRange);
			if (this.State != null) query.State(this.State);
			if (this.TaskIds != null) query.TaskIds(this.TaskIds);
			if (this.WorkflowRunIds != null) query.WorkflowRunIds(this.WorkflowRunIds);
			if (this.Pool != null) query.Pool(this.Pool);
			if (this.Queue != null) query.Queue(this.Queue);
			if (this.Executor != null) query.Executor(this.Executor);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<WorkflowTaskLookup>
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

			protected override IEnumerable<ISpecification> Specifications(WorkflowTaskLookup item)
			{
				return new ISpecification[]{
					//Workflow Ids must be null or not empty
					this.Spec()
						.Must(() => !item.WorkflowIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowTaskLookup.WorkflowIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowTaskLookup.WorkflowIds)]),
					//Paging with Ordering is only supported !
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(WorkflowTaskLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}