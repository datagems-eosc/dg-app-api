using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class UserCollection
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public User User { get; set; }
		public List<UserDatasetCollection> UserDatasetCollections { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public String ETag { get; set; }
	}

	public class UserCollectionPersist
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public String ETag { get; set; }

		public class PersistValidator : BaseValidator<UserCollectionPersist>
		{
			private static int NameMaxLength = typeof(Data.UserCollection).MaxLengthOf(nameof(Data.UserCollection.Name));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(UserCollectionPersist item)
			{
				return new ISpecification[]{
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(UserCollectionPersist.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(UserCollectionPersist.ETag)).FailWith(this._localizer["validation_required", nameof(UserCollectionPersist.ETag)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(UserCollectionPersist.Name)).FailWith(this._localizer["validation_required", nameof(UserCollectionPersist.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, PersistValidator.NameMaxLength))
						.FailOn(nameof(UserCollectionPersist.Name)).FailWith(this._localizer["validation_maxLength", nameof(UserCollectionPersist.Name)]),
				};
			}
		}
	}

	public class UserCollectionPersistDeep
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public List<UserDatasetCollectionPersist> UserDatasetCollections { get; set; }
		public String ETag { get; set; }

		public class PersistValidator : BaseValidator<UserCollectionPersistDeep>
		{
			private static int NameMaxLength = typeof(Data.UserCollection).MaxLengthOf(nameof(Data.UserCollection.Name));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(UserCollectionPersistDeep item)
			{
				return new ISpecification[]{
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(UserCollectionPersistDeep.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(UserCollectionPersistDeep.ETag)).FailWith(this._localizer["validation_required", nameof(UserCollectionPersistDeep.ETag)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(UserCollectionPersistDeep.Name)).FailWith(this._localizer["validation_required", nameof(UserCollectionPersistDeep.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, PersistValidator.NameMaxLength))
						.FailOn(nameof(UserCollectionPersistDeep.Name)).FailWith(this._localizer["validation_maxLength", nameof(UserCollectionPersistDeep.Name)]),
					//dataset collections must be valid if set
					this.NavSpec()
						.If(() => item.UserDatasetCollections != null)
						.On(nameof(UserCollectionPersistDeep.UserDatasetCollections))
						.Over(item.UserDatasetCollections)
						.Using(()=>_validatorFactory[typeof(UserDatasetCollectionPersist.PersistSubDeepValidator)]),
				};
			}
		}
	}

	public class UserCollectionDatasetPatch
	{
		public Guid? Id { get; set; }
		public List<UserDatasetCollectionPersist> UserDatasetCollections { get; set; }
		public String ETag { get; set; }

		public class PatchValidator : BaseValidator<UserCollectionDatasetPatch>
		{
			public PatchValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PatchValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(UserCollectionDatasetPatch item)
			{
				return new ISpecification[]{
					//id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(UserCollectionDatasetPatch.Id)).FailWith(this._localizer["validation_required", nameof(UserCollectionDatasetPatch.Id)]),
					//hash must always be set
					this.Spec()
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(UserCollectionDatasetPatch.ETag)).FailWith(this._localizer["validation_required", nameof(UserCollectionDatasetPatch.ETag)]),
					//attributes must be valid if set
					this.NavSpec()
						.If(() => item.UserDatasetCollections != null)
						.On(nameof(UserCollectionDatasetPatch.UserDatasetCollections))
						.Over(item.UserDatasetCollections)
						.Using(()=>this._validatorFactory[typeof(UserDatasetCollectionPersist.PersistSubDeepValidator)]),
				};
			}
		}
	}
}
