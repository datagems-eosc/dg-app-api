using Cite.Tools.Validation;
using DataGEMS.Gateway.App.ErrorCode;
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
					.Must(() => item.Url.IsValidHttp())
					.FailOn(nameof(DataLocation.Url)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Url)]),
				this.Spec()
					.If(() => item.Kind == DataLocationKind.Ftp)
					.Must(() => item.Url.IsValidFtp())
					.FailOn(nameof(DataLocation.Url)).FailWith(this._localizer["validation_invalidValue", nameof(DataLocation.Url)]),
			];
		}
	}
}
