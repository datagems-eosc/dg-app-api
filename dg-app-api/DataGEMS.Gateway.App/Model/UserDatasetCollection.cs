using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class UserDatasetCollection
	{
		public Guid? Id { get; set; }
		public Dataset Dataset { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public UserCollection UserCollection { get; set; }
		public String ETag { get; set; }
	}

	public class UserDatasetCollectionPersist
	{
		public Guid? Id { get; set; }
		public Guid? UserCollectionId { get; set; }
		public Guid? DatasetId { get; set; }
		public String ETag { get; set; }

		public class PersistValidator : BaseValidator<UserDatasetCollectionPersist>
		{
			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistSubDeepValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(UserDatasetCollectionPersist item)
			{
				return new ISpecification[]{
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(UserDatasetCollectionPersist.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(UserDatasetCollectionPersist.ETag)).FailWith(this._localizer["validation_required", nameof(UserDatasetCollectionPersist.ETag)]),
					//user collection must always be set
					this.Spec()
						.Must(() => !this.IsValidGuid(item.UserCollectionId))
						.FailOn(nameof(UserDatasetCollectionPersist.UserCollectionId)).FailWith(this._localizer["validation_required", nameof(UserDatasetCollectionPersist.UserCollectionId)]),
					//dataset must always be set
					this.Spec()
						.Must(() => !this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(UserDatasetCollectionPersist.DatasetId)).FailWith(this._localizer["validation_required", nameof(UserDatasetCollectionPersist.DatasetId)]),
				};
			}
		}

		public class PersistSubDeepValidator : BaseValidator<UserDatasetCollectionPersist>
		{
			public PersistSubDeepValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistSubDeepValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(UserDatasetCollectionPersist item)
			{
				return new ISpecification[]{
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(UserDatasetCollectionPersist.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(UserDatasetCollectionPersist.ETag)).FailWith(this._localizer["validation_required", nameof(UserDatasetCollectionPersist.ETag)]),
					//user collection must not be set
					this.Spec()
						.Must(() => !this.IsValidGuid(item.UserCollectionId))
						.FailOn(nameof(UserDatasetCollectionPersist.UserCollectionId)).FailWith(this._localizer["validation_overPosting"]),
					//dataset must always be set
					this.Spec()
						.Must(() => !this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(UserDatasetCollectionPersist.DatasetId)).FailWith(this._localizer["validation_required", nameof(UserDatasetCollectionPersist.DatasetId)]),
				};
			}
		}
	}
}
