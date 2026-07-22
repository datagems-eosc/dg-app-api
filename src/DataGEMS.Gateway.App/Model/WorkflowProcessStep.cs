using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Enum;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowProcessStep
	{
		public Guid? Id { get; set; }
		public WorkflowProcess Process { get; set; }
		public Guid? StepId { get; set; }
		public string WorkflowTaskInstanceDetails { get; set; }
		public WorkflowProcessStatus? Status { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	public class WorkflowProcessStepPersist
	{
		public Guid? Id { get; set; }
		public Guid? ProcessId { get; set; }
		public Guid? StepId { get; set; }
		public string WorkflowTaskInstanceDetails { get; set; }
		public WorkflowProcessStatus? Status { get; set; }

		public class PersistValidator : BaseValidator<WorkflowProcessStepPersist>
		{
			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}
			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
			protected override IEnumerable<ISpecification> Specifications(WorkflowProcessStepPersist item)
			{
				return [
					//Id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(WorkflowProcessStepPersist.Id)).FailWith(this._localizer["validation_required", nameof(WorkflowProcessStepPersist.Id)]),
					//ProcessId must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.ProcessId))
						.FailOn(nameof(WorkflowProcessStepPersist.ProcessId)).FailWith(this._localizer["validation_required", nameof(WorkflowProcessStepPersist.ProcessId)]),
					//StepId must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.StepId))
						.FailOn(nameof(WorkflowProcessStepPersist.StepId)).FailWith(this._localizer["validation_required", nameof(WorkflowProcessStepPersist.StepId)]),
					//Status must always be set
					this.Spec()
						.Must(() => item.Status != null)
						.FailOn(nameof(WorkflowProcessStepPersist.Status)).FailWith(this._localizer["validation_required", nameof(WorkflowProcessStepPersist.Status)]),
				];
			}
		}
	}
}
