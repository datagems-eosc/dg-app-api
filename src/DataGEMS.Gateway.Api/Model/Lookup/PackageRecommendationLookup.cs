using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class PackageRecommendationLookup
	{
		public int PackagesCount { get; set; }
		public int DatasetsPerPackage { get; set; }
		public List<Guid> DatasetIds { get; set; }
		public Cite.Tools.FieldSet.FieldSet Project { get; set; }


		public class PackageRecommendationLookupValidator : BaseValidator<PackageRecommendationLookup>
		{
			public PackageRecommendationLookupValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PackageRecommendationLookupValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(PackageRecommendationLookup item)
			{
				return [
					// dataset ids must not be null or empty
					this.Spec()
						.Must(() => item.DatasetIds != null && item.DatasetIds.Count > 0)
						.FailOn(nameof(PackageRecommendationLookup.DatasetIds))
						.FailWith(this._localizer["validation_required", nameof(PackageRecommendationLookup.DatasetIds)]),
					// packages count must be greater than 0
					this.Spec()
						.Must(() => item.PackagesCount > 0)
						.FailOn(nameof(PackageRecommendationLookup.PackagesCount))
						.FailWith(this._localizer["validation_positive_integer", nameof(PackageRecommendationLookup.PackagesCount)]),
					// datasets per package must be greater than 0
					this.Spec()
						.Must(() => item.DatasetsPerPackage > 0)
						.FailOn(nameof(PackageRecommendationLookup.DatasetsPerPackage))
						.FailWith(this._localizer["validation_positive_integer", nameof(PackageRecommendationLookup.DatasetsPerPackage)]),
					//The product of packages times the datasets per package must be less than or equal to the number of dataset ids provided
					this.Spec()
						.Must(() => item.PackagesCount * item.DatasetsPerPackage <= item.DatasetIds.Count)
						.FailOn(nameof(PackageRecommendationLookup.PackagesCount))
						.FailWith(this._localizer["validation_tooFew_datasets", item.DatasetIds.Count, item.PackagesCount * item.DatasetsPerPackage]),
				];
			}
		}
	}
}

