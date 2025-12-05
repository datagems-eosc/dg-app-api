using Cite.Tools.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Service.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Common.Validation
{
	public class DataLocationValidator : BaseValidator<DataLocation>
	{
		public DataLocationValidator(
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ValidatorFactory validatorFactory,
			ILogger<DataLocationValidator> logger,
			ErrorThesaurus errors,
			IStorageService storageService) : base(validatorFactory, logger, errors)
		{
			this._localizer = localizer;
			this._storageService = storageService;
		}
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
		private readonly IStorageService _storageService;

		protected override IEnumerable<ISpecification> Specifications(DataLocation item)
		{
			return [
				//Location must always be set
				this.Spec()
					.Must(() => !this.IsEmpty(item.Location))
					.FailOn(nameof(DataLocation.Location)).FailWith(this._localizer["validation_required", nameof(DataLocation.Location)]),
				// if kind is File, then string must be a valid path; if Kind is Http, then string must be a valid Location; If kind is Ftp, likewise.
				this.Spec()
					.If(() => item.Kind == DataLocationKind.File)
					.Must(() => item.Location.IsValidPath())
					.FailOn(nameof(DataLocation.Location)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Location)]),
				this.Spec()
					.If(() => item.Kind == DataLocationKind.Http)
					.Must(() => item.Location.IsValidHttp())
					.FailOn(nameof(DataLocation.Location)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Location)]),
				this.Spec()
					.If(() => item.Kind == DataLocationKind.Ftp)
					.Must(() => item.Location.IsValidFtp())
					.FailOn(nameof(DataLocation.Location)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Location)]),
				this.Spec()
					.If(() => item.Kind == DataLocationKind.Staged)
					.Must(() => this.IsValidGuid(item.Location))
					.FailOn(nameof(DataLocation.Location)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Location)]),
				//if data location is Staged, the directory must exist
				this.Spec()
					.If(() => item.Kind == DataLocationKind.Staged)
					.Must(() => this._storageService.DirectoryExists(StorageType.DatasetOnboardStaging, item.Location))
					.FailOn(nameof(DataLocation.Location)).FailWith(this._localizer["validation_stagedDataStoreNotExists"]),
			];
		}
	}
}
