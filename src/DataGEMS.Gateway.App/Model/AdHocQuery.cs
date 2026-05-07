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

		public Dataset Dataset { get; set; }

		public User User { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		public string AnalyticalPattern { get; set; }
	}

	public class AdHocQueryEvaluate
	{
		public string Query { get; set; }
		public Guid? DatasetId { get; set; }
		public Guid? DatabaseConnectionId { get; set; }
		public Dictionary<Guid, string> Arguments { get; set; }

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
					//dataset id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(AdHocQueryEvaluate.DatasetId)).FailWith(this._localizer["validation_required", nameof(AdHocQueryEvaluate.DatasetId)]),
					//database connection id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatabaseConnectionId))
						.FailOn(nameof(AdHocQueryEvaluate.DatabaseConnectionId)).FailWith(this._localizer["validation_required", nameof(AdHocQueryEvaluate.DatabaseConnectionId)]),
				];
			}
		}
	}
}
