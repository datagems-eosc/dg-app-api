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
	public class WorkflowTaskLogsLookup : Cite.Tools.Data.Query.Lookup
	{

		[SwaggerSchema("DAG Id of the task instance")]
		public string DagId { get; set; }

		[SwaggerSchema("Dag Run Id of the task instance")]
		public string DagRunId { get; set; }

		[SwaggerSchema("Task Id of the task instance")]
		public string TaskId { get; set; }

		[SwaggerSchema("The try number of the task run (>= 0)")]
		public int TryNumber { get; set; }

		[SwaggerSchema("Return full log content default: false")]
		public bool FullContent { get; set; } 

		[SwaggerSchema("Map Index for mapped tasks default: -1 ")]
		public int MapIndex { get; set; } 

		[SwaggerSchema("Continuation token for paginated logs")]
		public string? Token { get; set; }

		public WorkflowTaskLogsHttpQuery Enrich(QueryFactory factory)
		{
			WorkflowTaskLogsHttpQuery query = factory.Query<WorkflowTaskLogsHttpQuery>();

			if (this.DagId != null) query.DagIds(this.DagId);
			
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
