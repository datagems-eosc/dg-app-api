using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;

namespace DataGEMS.Gateway.Api.Model
{
	public class CrossDatasetDiscoveryLookup
	{
		public String Query { get; set; }
		public int? ResultCount { get; set; }
	}

	public class CrossDatasetDiscoveryLookupValidator : BaseValidator<CrossDatasetDiscoveryLookup>
	{
		public CrossDatasetDiscoveryLookupValidator(
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources>  localizer,
			ValidatorFactory validatorFactory,
			ILogger<CrossDatasetDiscoveryLookupValidator> logger,
			ErrorThesaurus errors) : base(validatorFactory, logger, errors)
		{
			this._localizer = localizer;
		}

		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		protected override IEnumerable<ISpecification> Specifications(CrossDatasetDiscoveryLookup item)
		{
			return new ISpecification[] { 
				// Query must always be set
				this.Spec()
					.Must(() => !this.IsEmpty(item.Query))
					.FailOn(nameof(CrossDatasetDiscoveryLookup.Query))
					.FailWith(this._localizer["validation_required", nameof(CrossDatasetDiscoveryLookup.Query)]),
				// If ResultCount is provided, must be > 0
				this.Spec()
					.If(() => item.ResultCount.HasValue)
					.Must(() => item.ResultCount.Value > 0)
					.FailOn(nameof(CrossDatasetDiscoveryLookup.ResultCount))
					.FailWith(this._localizer["validation_positive", nameof(CrossDatasetDiscoveryLookup.ResultCount)]),
			};
		}

	}
}
