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

	public class WorkflowOnboardingStepFinalize
	{
		public WorkflowProcessStepPersist WorkflowProcessStep { get; set; }
		public DatasetProfiling Profiling { get; set; }

		public class Validator: BaseValidator<WorkflowOnboardingStepFinalize>
		{
			public Validator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<Validator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}
			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
			protected override IEnumerable<ISpecification> Specifications(WorkflowOnboardingStepFinalize item)
			{
				return [
					//WorkflowProcessStepPersist must not be null
					this.Spec()
						.Must(() => item.WorkflowProcessStep != null)
						.FailOn(nameof(WorkflowOnboardingStepFinalize.WorkflowProcessStep)).FailWith(this._localizer["validation_required", nameof(WorkflowOnboardingStepFinalize.WorkflowProcessStep)]),
					//WorkflowProcessStepPersist must be valid
					this.RefSpec()
						.If(() => item.WorkflowProcessStep != null)
						.On(nameof(WorkflowOnboardingStepFinalize.WorkflowProcessStep))
						.Over(item.WorkflowProcessStep)
						.Using(() => _validatorFactory[typeof(WorkflowProcessStepPersist.PersistValidator)]),
					//Profiling must not be null
					this.Spec()
						.Must(() => item.Profiling != null)
						.FailOn(nameof(WorkflowOnboardingStepFinalize.Profiling)).FailWith(this._localizer["validation_required", nameof(WorkflowOnboardingStepFinalize.Profiling)]),
					//DatasetProfiling must be valid
					this.RefSpec()
						.If(() => item.Profiling != null)
						.On(nameof(WorkflowOnboardingStepFinalize.Profiling))
						.Over(item.Profiling)
						.Using(() => _validatorFactory[typeof(DatasetProfiling.ProfilingValidator)]),
				];
			}
		}
	}

	public class WorkflowProfilingStepFinalize
	{
		public WorkflowProcessStepPersist WorkflowProcessStep { get; set; }
		public Guid? DatasetId { get; set; }

		public class Validator : BaseValidator<WorkflowProfilingStepFinalize>
		{
			public Validator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<Validator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}
			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
			protected override IEnumerable<ISpecification> Specifications(WorkflowProfilingStepFinalize item)
			{
				return [
					//WorkflowProcessStepPersist must not be null
					this.Spec()
						.Must(() => item.WorkflowProcessStep != null)
						.FailOn(nameof(WorkflowProfilingStepFinalize.WorkflowProcessStep)).FailWith(this._localizer["validation_required", nameof(WorkflowProfilingStepFinalize.WorkflowProcessStep)]),
					//WorkflowProcessStepPersist must be valid
					this.RefSpec()
						.If(() => item.WorkflowProcessStep != null)
						.On(nameof(WorkflowProfilingStepFinalize.WorkflowProcessStep))
						.Over(item.WorkflowProcessStep)
						.Using(() => _validatorFactory[typeof(WorkflowProcessStepPersist.PersistValidator)]),
					//DatasetId must be valid
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(WorkflowProfilingStepFinalize.DatasetId)).FailWith(this._localizer["validation_required", nameof(WorkflowProfilingStepFinalize.DatasetId)]),
				];
			}
		}
	}

	public class WorkflowPackagingStepFinalize
	{
		public WorkflowProcessStepPersist WorkflowProcessStep { get; set; }
		public Guid? DatasetId { get; set; }

		public class Validator : BaseValidator<WorkflowPackagingStepFinalize>
		{
			public Validator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<Validator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}
			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
			protected override IEnumerable<ISpecification> Specifications(WorkflowPackagingStepFinalize item)
			{
				return [
					//WorkflowProcessStepPersist must not be null
					this.Spec()
						.Must(() => item.WorkflowProcessStep != null)
						.FailOn(nameof(WorkflowPackagingStepFinalize.WorkflowProcessStep)).FailWith(this._localizer["validation_required", nameof(WorkflowPackagingStepFinalize.WorkflowProcessStep)]),
					//WorkflowProcessStepPersist must be valid
					this.RefSpec()
						.If(() => item.WorkflowProcessStep != null)
						.On(nameof(WorkflowPackagingStepFinalize.WorkflowProcessStep))
						.Over(item.WorkflowProcessStep)
						.Using(() => _validatorFactory[typeof(WorkflowProcessStepPersist.PersistValidator)]),
					//DatasetId must be valid
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(WorkflowPackagingStepFinalize.DatasetId)).FailWith(this._localizer["validation_required", nameof(WorkflowPackagingStepFinalize.DatasetId)]),
				];
			}
		}
	}

	public class WorkflowRecommendationStepFinalize
	{
		public WorkflowProcessStepPersist WorkflowProcessStep { get; set; }
		public Guid? DatasetId { get; set; }

		public class Validator : BaseValidator<WorkflowRecommendationStepFinalize>
		{
			public Validator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<Validator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}
			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
			protected override IEnumerable<ISpecification> Specifications(WorkflowRecommendationStepFinalize item)
			{
				return [
					//WorkflowProcessStepPersist must not be null
					this.Spec()
						.Must(() => item.WorkflowProcessStep != null)
						.FailOn(nameof(WorkflowRecommendationStepFinalize.WorkflowProcessStep)).FailWith(this._localizer["validation_required", nameof(WorkflowRecommendationStepFinalize.WorkflowProcessStep)]),
					//WorkflowProcessStepPersist must be valid
					this.RefSpec()
						.If(() => item.WorkflowProcessStep != null)
						.On(nameof(WorkflowRecommendationStepFinalize.WorkflowProcessStep))
						.Over(item.WorkflowProcessStep)
						.Using(() => _validatorFactory[typeof(WorkflowProcessStepPersist.PersistValidator)]),
					//DatasetId must be valid
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(WorkflowRecommendationStepFinalize.DatasetId)).FailWith(this._localizer["validation_required", nameof(WorkflowRecommendationStepFinalize.DatasetId)]),
				];
			}
		}
	}

	public class WorkflowCddIngestionStepFinalize
	{
		public WorkflowProcessStepPersist WorkflowProcessStep { get; set; }
		public Guid? DatasetId { get; set; }

		public class Validator : BaseValidator<WorkflowCddIngestionStepFinalize>
		{
			public Validator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<Validator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}
			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
			protected override IEnumerable<ISpecification> Specifications(WorkflowCddIngestionStepFinalize item)
			{
				return [
					//WorkflowProcessStepPersist must not be null
					this.Spec()
						.Must(() => item.WorkflowProcessStep != null)
						.FailOn(nameof(WorkflowCddIngestionStepFinalize.WorkflowProcessStep)).FailWith(this._localizer["validation_required", nameof(WorkflowCddIngestionStepFinalize.WorkflowProcessStep)]),
					//WorkflowProcessStepPersist must be valid
					this.RefSpec()
						.If(() => item.WorkflowProcessStep != null)
						.On(nameof(WorkflowCddIngestionStepFinalize.WorkflowProcessStep))
						.Over(item.WorkflowProcessStep)
						.Using(() => _validatorFactory[typeof(WorkflowProcessStepPersist.PersistValidator)]),
					//DatasetId must be valid
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(WorkflowCddIngestionStepFinalize.DatasetId)).FailWith(this._localizer["validation_required", nameof(WorkflowCddIngestionStepFinalize.DatasetId)]),
				];
			}
		}
	}
}
