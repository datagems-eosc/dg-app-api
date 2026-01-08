using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class TaskInstanceDownstreamExecutionArgs
	{
		public string WorkflowId { get; set; }
		public List<string> TaskIds { get; set; }
		public string WorkflowExecutionId { get; set; }

		public class TaskInstanceDownstreamExecutionArgsValidator : BaseValidator<TaskInstanceDownstreamExecutionArgs>
		{
			public TaskInstanceDownstreamExecutionArgsValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<TaskInstanceDownstreamExecutionArgsValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(TaskInstanceDownstreamExecutionArgs item)
			{
				return [
					//workflow id must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.WorkflowId))
						.FailOn(nameof(TaskInstanceDownstreamExecutionArgs.WorkflowId)).FailWith(this._localizer["validation_required", nameof(TaskInstanceDownstreamExecutionArgs.WorkflowId)]),
					//workflow execution id must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.WorkflowExecutionId))
						.FailOn(nameof(TaskInstanceDownstreamExecutionArgs.WorkflowExecutionId)).FailWith(this._localizer["validation_required", nameof(TaskInstanceDownstreamExecutionArgs.WorkflowExecutionId)]),
					//task ids must always be set and contain at least one id
					this.Spec()
						.Must(() => item.TaskIds != null && item.TaskIds.Any())
						.FailOn(nameof(TaskInstanceDownstreamExecutionArgs.TaskIds)).FailWith(this._localizer["validation_required", nameof(TaskInstanceDownstreamExecutionArgs.TaskIds)]),
				];
			}
		}
	}
}
