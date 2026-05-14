using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class AdHocQuery
	{
		public Guid? Id { get; set; }
		public User User { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		public string AnalyticalPattern { get; set; }
	}

	public class AdHocQueryEvaluateArgument
	{
		public string ArgName { get; set; }
		public Guid? FileObjectId { get; set; }
		public Guid? DatasetId { get; set; }
		public Guid? DatabaseConnectionId { get; set; }

		public class EvaluateArgumentValidator : BaseValidator<AdHocQueryEvaluateArgument>
		{
			public EvaluateArgumentValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<EvaluateArgumentValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}
			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
			protected override IEnumerable<ISpecification> Specifications(AdHocQueryEvaluateArgument item)
			{
				return [
					//argument name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.ArgName))
						.FailOn(nameof(AdHocQueryEvaluateArgument.ArgName)).FailWith(this._localizer["validation_required", nameof(AdHocQueryEvaluateArgument.ArgName)]),
					//dataset id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(AdHocQueryEvaluateArgument.DatasetId)).FailWith(this._localizer["validation_required", nameof(AdHocQueryEvaluateArgument.DatasetId)]),
					//file object id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.FileObjectId))
						.FailOn(nameof(AdHocQueryEvaluateArgument.FileObjectId)).FailWith(this._localizer["validation_required", nameof(AdHocQueryEvaluateArgument.FileObjectId)]),
				];
			}
		}
	}

	public class AdHocQueryEvaluate
	{
		public string Query { get; set; }
		public List<AdHocQueryEvaluateArgument> Arguments { get; set; }

		public class EvaluateValidator : BaseValidator<AdHocQueryEvaluate>
		{
			public EvaluateValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<EvaluateValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(AdHocQueryEvaluate item)
			{
				return [
					//query must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Query))
						.FailOn(nameof(AdHocQueryEvaluate.Query)).FailWith(this._localizer["validation_required", nameof(AdHocQueryEvaluate.Query)]),
					// arguments must be valid
					 this.NavSpec()
						.If(() => item.Arguments != null)
						.On(nameof(AdHocQueryEvaluate.Arguments))
						.Over(item.Arguments)
						.Using(() => _validatorFactory[typeof(AdHocQueryEvaluateArgument.EvaluateArgumentValidator)]),
				];
			}
		}
	}
}
