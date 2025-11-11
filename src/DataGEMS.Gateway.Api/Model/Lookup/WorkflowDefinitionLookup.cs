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
	public class WorkflowDefinitionLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific kinds. If set, the list of values must not be empty")]
		public List<WorkflowDefinitionKind> Kinds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose name matches the pattern")]
		public String Like { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are or are not staled")]
		public Boolean? ExcludeStaled { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that are or are not paused")]
		public Boolean? OnlyPaused { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose last run was at a specific state")]
		public WorkflowRunState? LastRunState { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose run start is in the specific period")]
		public RangeOf<DateOnly?> RunStartRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose run end is in the specific period")]
		public RangeOf<DateOnly?> RunEndRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items who are at a specific state. If set, the list of values must not be empty")]
		public List<WorkflowRunState> RunState { get; set; }

		public WorkflowDefinitionHttpQuery Enrich(QueryFactory factory)
		{
			WorkflowDefinitionHttpQuery query = factory.Query<WorkflowDefinitionHttpQuery>();

			if (this.Kinds != null) query.Kinds(this.Kinds);
			if (!String.IsNullOrEmpty(this.Like)) query.Like(this.Like);
			if (this.ExcludeStaled.HasValue) query.ExcludeStaled(this.ExcludeStaled);
			if (this.OnlyPaused.HasValue) query.OnlyPaused(this.OnlyPaused);
			if (this.LastRunState.HasValue) query.LastRunState(this.LastRunState.Value);
			if (this.RunStartRange != null) query.RunStartRange(this.RunStartRange);
			if (this.RunEndRange != null) query.RunEndRange(this.RunEndRange);
			if (this.RunState != null) query.RunStates(this.RunState);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<WorkflowDefinitionLookup>
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

			protected override IEnumerable<ISpecification> Specifications(WorkflowDefinitionLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Kinds.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowDefinitionLookup.Kinds)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowDefinitionLookup.Kinds)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.RunState.IsNotNullButEmpty())
						.FailOn(nameof(WorkflowDefinitionLookup.RunState)).FailWith(this._localizer["validation_setButEmpty", nameof(WorkflowDefinitionLookup.RunState)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(WorkflowDefinitionLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
