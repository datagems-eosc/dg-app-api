using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class UserFavorite
	{
		public Guid? Id { get; set; }
		public Dataset Dataset { get; set; }
		public User User { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	public class UserFavoritePersist
	{
		public Guid? DatasetId { get; set; }

		public class PersistValidator : BaseValidator<UserFavoritePersist>
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

			protected override IEnumerable<ISpecification> Specifications(UserFavoritePersist item)
			{
				return [
					//dataset id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(UserFavoritePersist.DatasetId)).FailWith(this._localizer["validation_required", nameof(UserFavoritePersist.DatasetId)]),
				];
			}
		}
	}
}
