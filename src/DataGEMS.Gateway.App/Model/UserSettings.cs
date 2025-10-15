using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class UserSettings
	{
		public Guid? Id { get; set; }
		public String Key { get; set; }
		public User User { get; set; }
		public String Value { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public String ETag { get; set; }
	}

	public class UserSettingsPersist
	{
		public Guid? Id { get; set; }
		public String Key { get; set; }
		public String Value { get; set; }
		public String ETag { get; set; }

		public class PersistValidator : BaseValidator<UserSettingsPersist>
		{
			private static int KeyMaxLength = typeof(Data.UserSettings).MaxLengthOf(nameof(Data.UserSettings.Key));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(UserSettingsPersist item)
			{
				return new ISpecification[]{
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(UserSettingsPersist.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(UserSettingsPersist.ETag)).FailWith(this._localizer["validation_required", nameof(UserSettingsPersist.ETag)]),
					//key must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Key))
						.FailOn(nameof(UserSettingsPersist.Key)).FailWith(this._localizer["validation_required", nameof(UserSettingsPersist.Key)]),
					//key max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Key))
						.Must(() => this.LessEqual(item.Key, PersistValidator.KeyMaxLength))
						.FailOn(nameof(UserSettingsPersist.Key)).FailWith(this._localizer["validation_maxLength", nameof(UserSettingsPersist.Key)]),
					//value must be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Value))
						.FailOn(nameof(UserSettingsPersist.Value)).FailWith(this._localizer["validation_required", nameof(UserSettingsPersist.Value)]),
				};
			}
		}
	}
}
