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
	public class WorkflowTaskInstanceLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items that are in the Task Id List")]
		public List<String> TaskIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Workflow Id List")]
		public List<String> WorkflowIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are in the Workflow Run Id List")]
		public List<String> WorkflowExecutionIds { get; set; }
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
		public List<WorkflowTaskInstanceState> State { get; set; }

		public WorkflowTaskInstanceHttpQuery Enrich(QueryFactory factory)
		{
			WorkflowTaskInstanceHttpQuery query = factory.Query<WorkflowTaskInstanceHttpQuery>();

			if (this.TaskIds != null) query.TaskIds(this.TaskIds);
			if (this.WorkflowIds != null) query.WorkflowIds(this.WorkflowIds);
			if (this.WorkflowExecutionIds != null) query.WorkflowExecutionIds(this.WorkflowExecutionIds);
			if (this.State != null) query.State(this.State);
			if (this.RunAfterRange != null) query.RunAfterRange(this.RunAfterRange);
			if (this.LogicalDateRange != null) query.LogicalDateRange(this.LogicalDateRange);
			if (this.StartDateRange != null) query.StartDateRange(this.StartDateRange);
			if (this.EndDateRange != null) query.EndDateRange(this.EndDateRange);
			if (this.DurationRange != null) query.DurationRange(this.DurationRange);
			if (this.Pool != null) query.Pool(this.Pool);
			if (this.Queue != null) query.Queue(this.Queue);
			if (this.Executor != null) query.Executor(this.Executor);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<WorkflowTaskInstanceLookup>
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

			protected override IEnumerable<ISpecification> Specifications(WorkflowTaskInstanceLookup item)
			{
				return new ISpecification[]{
					//Task Ids must be null or not empty
					this.Spec()
						.Must(() => !item.TaskIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowTaskInstanceLookup.TaskIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowTaskInstanceLookup.TaskIds)]),
					//WorkflowRun Ids must be null or not empty
					this.Spec()
						.Must(() => !item.WorkflowExecutionIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowTaskInstanceLookup.WorkflowExecutionIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowTaskInstanceLookup.WorkflowExecutionIds)]),
					//Workflow Ids must be null or not empty
					this.Spec()
						.Must(() => !item.WorkflowIds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowTaskInstanceLookup.WorkflowIds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowTaskInstanceLookup.WorkflowIds)]),
					//Pool must be null or not empty
					this.Spec()
						.Must(() => !item.Pool.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowTaskInstanceLookup.Pool)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowTaskInstanceLookup.Pool)]),
					//Queue must be null or not empty
					this.Spec()
						.Must(() => !item.Queue.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowTaskInstanceLookup.Queue)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowTaskInstanceLookup.Queue)]),
					//Executor must be null or not empty
					this.Spec()
						.Must(() => !item.Executor.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowTaskInstanceLookup.Executor)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowTaskInstanceLookup.Executor)]),
					//Paging with Ordering is only supported !
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(WorkflowTaskInstanceLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}