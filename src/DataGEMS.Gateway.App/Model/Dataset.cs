
using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace DataGEMS.Gateway.App.Model
{
	public class Dataset
	{
		public class FeatureStatus
		{
			public bool Profiled { get; set; }
			public bool Packaged { get; set; }
			public bool Recommendation { get; set; }
		}

		public Guid? Id { get; set; }
		[MaxLength(250)]
		public String Name { get; set; }
		public String Description { get; set; }
		public String License { get; set; }
		[MaxLength(300)]
		public String Url { get; set; }
		public String Headline { get; set; }
		public List<String> Keywords { get; set; }
		public List<String> FieldOfScience { get; set; }
		public List<String> Language { get; set; }
		public List<String> Country { get; set; }
		public DateOnly? DatePublished { get; set; }
		public string ArchivedAt { get; set; }
		public string CiteAs { get; set; }
		public Object ProfileRaw { get; set; }
		public String Status { get; set; }
		public string Doi { get; set; }
		public List<Model.Collection> Collections { get; set; }
		public List<String> Permissions { get; set; }
		public FeatureStatus Features { get; set; }
	}

	public class DatasetPersist
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public String Description { get; set; }
		public String License { get; set; }
		public String Url { get; set; }
		public String Headline { get; set; }
		public List<String> Keywords { get; set; }
		public List<String> FieldOfScience { get; set; }
		public List<String> Language { get; set; }
		public List<String> Country { get; set; }
		public DateOnly? DatePublished { get; set; }
		public List<DataLocation> DataLocations { get; set; }
		public string CiteAs { get; set; }
		public string Doi { get; set; }

		public class OnboardValidator : BaseValidator<DatasetPersist>
		{
			private static int NameMaxLength = typeof(Dataset).MaxLengthOf(nameof(Dataset.Name));
			private static int UrlMaxLength = typeof(Dataset).MaxLengthOf(nameof(Dataset.Url));

			public OnboardValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<OnboardValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(DatasetPersist item)
			{
				return new ISpecification[]{
					//id must not be set
					this.Spec()
						.Must(() => !this.IsValidGuid(item.Id))
						.FailOn(nameof(DatasetPersist.Id)).FailWith(this._localizer["validation_overPosting", nameof(DatasetPersist.Id)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(DatasetPersist.Name)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, OnboardValidator.NameMaxLength))
						.FailOn(nameof(DatasetPersist.Name)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Name)]),
					//description must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Description))
						.FailOn(nameof(DatasetPersist.Description)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Description)]),
					//License must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.License))
						.FailOn(nameof(DatasetPersist.License)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.License)]),
					//Location must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Url))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Url)]),
					//Location max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Url))
						.Must(() => this.LessEqual(item.Url, OnboardValidator.UrlMaxLength))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Url)]),
					//Headline must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Headline))
						.FailOn(nameof(DatasetPersist.Headline)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Headline)]),
					//Keywords must always be set
					this.Spec()
						.Must(() => item.Keywords != null && item.Keywords.Count > 0)
						.FailOn(nameof(DatasetPersist.Keywords)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Keywords)]),
					//FieldOfScience must always be set
					this.Spec()
						.Must(() => item.FieldOfScience != null && item.FieldOfScience.Count > 0)
						.FailOn(nameof(DatasetPersist.FieldOfScience)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.FieldOfScience)]),
					//DatePublished must always be set
					this.Spec()
						.Must(() => item.DatePublished.HasValue)
						.FailOn(nameof(DatasetPersist.DatePublished)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.DatePublished)]),
					//data location must be set
					this.Spec()
						.Must(() => item.DataLocations != null && item.DataLocations.Count > 0)
						.FailOn(nameof(DatasetPersist.DataLocations)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.DataLocations)]),
					//data location must be valid
					this.NavSpec()
						.If(() => item.DataLocations != null)
						.On(nameof(DatasetPersist.DataLocations))
						.Over(item.DataLocations)
						.Using(()=>_validatorFactory[typeof(DataLocationValidator)]),
					//if data location is Staged, it must be only one
					this.Spec()
						.If(() => item.DataLocations != null && item.DataLocations.Any(x => x.Kind == DataLocationKind.Staged))
						.Must(() => item.DataLocations.Count() == 1)
						.FailOn(nameof(DatasetPersist.DataLocations)).FailWith(this._localizer["validation_onlyOneStagedDataStore"]),
				};
			}
		}

		public class PersistValidator : BaseValidator<DatasetPersist>
		{
			private static int NameMaxLength = typeof(Dataset).MaxLengthOf(nameof(Dataset.Name));
			private static int UrlMaxLength = typeof(Dataset).MaxLengthOf(nameof(Dataset.Url));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(DatasetPersist item)
			{
				return new ISpecification[]{
					//id must be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(DatasetPersist.Id)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Id)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(DatasetPersist.Name)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, PersistValidator.NameMaxLength))
						.FailOn(nameof(DatasetPersist.Name)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Name)]),
					//description must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Description))
						.FailOn(nameof(DatasetPersist.Description)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Description)]),
					//License must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.License))
						.FailOn(nameof(DatasetPersist.License)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.License)]),
					//Location must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Url))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Url)]),
					//Location max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Url))
						.Must(() => this.LessEqual(item.Url, PersistValidator.UrlMaxLength))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Url)]),
					//Headline must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Headline))
						.FailOn(nameof(DatasetPersist.Headline)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Headline)]),
					//Keywords must always be set
					this.Spec()
						.Must(() => item.Keywords != null && item.Keywords.Count > 0)
						.FailOn(nameof(DatasetPersist.Keywords)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Keywords)]),
					//FieldOfScience must always be set
					this.Spec()
						.Must(() => item.FieldOfScience != null || item.FieldOfScience.Count > 0)
						.FailOn(nameof(DatasetPersist.FieldOfScience)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.FieldOfScience)]),
					//DatePublished must always be set
					this.Spec()
						.Must(() => item.DatePublished.HasValue)
						.FailOn(nameof(DatasetPersist.DatePublished)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.DatePublished)]),
					//data location must not set
					this.Spec()
						.Must(() => item.DataLocations == null)
						.FailOn(nameof(DatasetPersist.DataLocations)).FailWith(this._localizer["validation_overPosting", nameof(DatasetPersist.DataLocations)]),
				};
			}
		}
	}

	public class DatasetProfiling
	{
		public Guid? Id { get; set; }
		public DataStoreKind? DataStoreKind { get; set; }
		public string DatabaseName { get; set; }


		public class ProfilingValidator : BaseValidator<DatasetProfiling>
		{
			public ProfilingValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<ProfilingValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(DatasetProfiling item)
			{
				return [
					//id must be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(DatasetProfiling.Id)).FailWith(this._localizer["validation_required", nameof(DatasetProfiling.Id)]),
					//DataStoreKind must always be set
					this.Spec()
						.Must(() => item.DataStoreKind.HasValue)
						.FailOn(nameof(DatasetProfiling.DataStoreKind)).FailWith(this._localizer["validation_required", nameof(DatasetProfiling.DataStoreKind)]),
				];
			}
		}
	}

	public class DatasetPackaging
	{
		public Guid? Id { get; set; }

		public class PackagingValidator : BaseValidator<DatasetPackaging>
		{
			public PackagingValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PackagingValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(DatasetPackaging item)
			{
				return [
					//id must be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(DatasetPackaging.Id)).FailWith(this._localizer["validation_required", nameof(DatasetPackaging.Id)]),
				];
			}
		}
	}

	public class DatasetRecommendationRegistering
	{
		public Guid? Id { get; set; }

		public class RecommendationRegisteringValidator : BaseValidator<DatasetRecommendationRegistering>
		{
			public RecommendationRegisteringValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<RecommendationRegisteringValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(DatasetRecommendationRegistering item)
			{
				return [
					//id must be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(DatasetRecommendationRegistering.Id)).FailWith(this._localizer["validation_required", nameof(DatasetRecommendationRegistering.Id)]),
				];
			}
		}
	}

}
