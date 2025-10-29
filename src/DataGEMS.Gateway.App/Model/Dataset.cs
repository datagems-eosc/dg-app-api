
using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class Dataset
	{
		public Guid? Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }
		public String Description { get; set; }
		public String License { get; set; }
		public String MimeType { get; set; }
		public long? Size { get; set; }
		public String Url { get; set; }
		public String Version { get; set; }
		public String Headline { get; set; }
		public List<String> Keywords { get; set; }
		public List<String> FieldOfScience { get; set; }
		public List<String> Language { get; set; }
		public List<String> Country { get; set; }
		public DateOnly? DatePublished { get; set; }
		public String ProfileRaw { get; set; }
		public List<Model.Collection> Collections { get; set; }
		public List<String> Permissions { get; set; }
	}

	public class DatasetPersist
	{
		public Guid? Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }
		public String Description { get; set; }
		public String License { get; set; }
		public String MimeType { get; set; }
		public long? Size { get; set; }
		public String Url { get; set; }
		public String Version { get; set; }
		public String Headline { get; set; }
		public List<String> Keywords { get; set; }
		public List<String> FieldOfScience { get; set; }
		public List<String> Language { get; set; }
		public List<String> Country { get; set; }
		public DateOnly? DatePublished { get; set; }
		public Common.DataLocation DataLocation { get; set; }

		public class OnboardValidator : BaseValidator<DatasetPersist>
		{
			private static int CodeMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Code));
			private static int NameMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Name));
			private static int UrlMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Url));
			private static int VersionMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Version));
			private static int MimeTypeMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.MimeType));

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
					//code must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Code))
						.FailOn(nameof(DatasetPersist.Code)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Code)]),
					//code max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Code))
						.Must(() => this.LessEqual(item.Code, OnboardValidator.CodeMaxLength))
						.FailOn(nameof(DatasetPersist.Code)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Code)]),
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
					//MimeType must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.MimeType))
						.FailOn(nameof(DatasetPersist.MimeType)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.MimeType)]),
					//VersMimeTypeion max length
					this.Spec()
						.If(() => !this.IsEmpty(item.MimeType))
						.Must(() => this.LessEqual(item.MimeType, OnboardValidator.MimeTypeMaxLength))
						.FailOn(nameof(DatasetPersist.MimeType)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.MimeType)]),
					//Size must always be set
					this.Spec()
						.Must(() => item.Size.HasValue)
						.FailOn(nameof(DatasetPersist.Size)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Size)]),
					//Url must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Url))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Url)]),
					//Url max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Url))
						.Must(() => this.LessEqual(item.Url, OnboardValidator.UrlMaxLength))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Url)]),
					//Version max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Version))
						.Must(() => this.LessEqual(item.Version, OnboardValidator.VersionMaxLength))
						.FailOn(nameof(DatasetPersist.Version)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Version)]),
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
					//Language must always be set
					this.Spec()
						.Must(() => item.Language != null || item.Language.Count > 0)
						.FailOn(nameof(DatasetPersist.Language)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Language)]),
					//Country must always be set
					this.Spec()
						.Must(() => item.Country != null || item.Country.Count > 0)
						.FailOn(nameof(DatasetPersist.Country)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Country)]),
					//DatePublished must always be set
					this.Spec()
						.Must(() => item.DatePublished.HasValue)
						.FailOn(nameof(DatasetPersist.DatePublished)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.DatePublished)]),
					//data location must be set
					this.Spec()
						.Must(() => item.DataLocation != null)
						.FailOn(nameof(DatasetPersist.DataLocation)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.DataLocation)]),
					//data location must be valid
					this.RefSpec()
						.If(() => item.DataLocation != null)
						.On(nameof(DatasetPersist.DataLocation))
						.Over(item.DataLocation)
						.Using(()=>_validatorFactory[typeof(DataLocationPersistValidator)]),
				};
			}
		}

		public class PersistValidator : BaseValidator<DatasetPersist>
		{
			private static int CodeMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Code));
			private static int NameMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Name));
			private static int UrlMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Url));
			private static int VersionMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.Version));
			private static int MimeTypeMaxLength = typeof(Service.DataManagement.Data.Dataset).MaxLengthOf(nameof(Service.DataManagement.Data.Dataset.MimeType));

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
					//code must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Code))
						.FailOn(nameof(DatasetPersist.Code)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Code)]),
					//code max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Code))
						.Must(() => this.LessEqual(item.Code, PersistValidator.CodeMaxLength))
						.FailOn(nameof(DatasetPersist.Code)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Code)]),
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
					//MimeType must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.MimeType))
						.FailOn(nameof(DatasetPersist.MimeType)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.MimeType)]),
					//VersMimeTypeion max length
					this.Spec()
						.If(() => !this.IsEmpty(item.MimeType))
						.Must(() => this.LessEqual(item.MimeType, PersistValidator.MimeTypeMaxLength))
						.FailOn(nameof(DatasetPersist.MimeType)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.MimeType)]),
					//Size must always be set
					this.Spec()
						.Must(() => item.Size.HasValue)
						.FailOn(nameof(DatasetPersist.Size)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Size)]),
					//Url must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Url))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Url)]),
					//Url max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Url))
						.Must(() => this.LessEqual(item.Url, PersistValidator.UrlMaxLength))
						.FailOn(nameof(DatasetPersist.Url)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Url)]),
					//Version max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Version))
						.Must(() => this.LessEqual(item.Version, PersistValidator.VersionMaxLength))
						.FailOn(nameof(DatasetPersist.Version)).FailWith(this._localizer["validation_maxLength", nameof(DatasetPersist.Version)]),
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
					//Language must always be set
					this.Spec()
						.Must(() => item.Language != null || item.Language.Count > 0)
						.FailOn(nameof(DatasetPersist.Language)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Language)]),
					//Country must always be set
					this.Spec()
						.Must(() => item.Country != null || item.Country.Count > 0)
						.FailOn(nameof(DatasetPersist.Country)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.Country)]),
					//DatePublished must always be set
					this.Spec()
						.Must(() => item.DatePublished.HasValue)
						.FailOn(nameof(DatasetPersist.DatePublished)).FailWith(this._localizer["validation_required", nameof(DatasetPersist.DatePublished)]),
				};
			}
		}
	}

	public class DataLocationPersistValidator : BaseValidator<DataLocation>
	{
		public DataLocationPersistValidator(
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ValidatorFactory validatorFactory,
			ILogger<DataLocationPersistValidator> logger,
			ErrorThesaurus errors) : base(validatorFactory, logger, errors)
		{
			this._localizer = localizer;
		}
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		protected override IEnumerable<ISpecification> Specifications(DataLocation item)
		{
			return [
				//Url must always be set
				this.Spec()
						.Must(() => !this.IsEmpty(item.Url))
						.FailOn(nameof(DataLocation.Url)).FailWith(this._localizer["validation_required", nameof(DataLocation.Url)]),
					// if kind is File, then string must be a valid path; if Kind is Http, then string must be a valid Url; If kind is Ftp, likewise.
					this.Spec()
						.If(() => item.Kind == DataLocationKind.File)
						.Must(() => item.Url.IsValidPath())
						.FailOn(nameof(DataLocation.Url)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Url)]),
					this.Spec()
						.If(() => item.Kind == DataLocationKind.Http)
						.Must(() => item.Url.IsValidUrl())
						.FailOn(nameof(DataLocation.Url)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Url)]),
					this.Spec()
						.If(() => item.Kind == DataLocationKind.Ftp)
						.Must(() => item.Url.IsValidFtp())
						.FailOn(nameof(DataLocation.Url)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Url)]),
				];
		}
	}
}
