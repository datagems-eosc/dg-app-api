
using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
    public class Collection
	{
		public Guid? Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }
		public List<Model.Dataset> Datasets { get; set; }
		public int? DatasetCount { get; set; }
		public List<String> Permissions { get; set; }
	}

	public class CollectionPersist
	{
		public Guid? Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }

		public class PersistValidator : BaseValidator<CollectionPersist>
		{
			private static int CodeMaxLength = typeof(Service.DataManagement.Data.Collection).MaxLengthOf(nameof(Service.DataManagement.Data.Collection.Code));
			private static int NameMaxLength = typeof(Service.DataManagement.Data.Collection).MaxLengthOf(nameof(Service.DataManagement.Data.Collection.Name));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(CollectionPersist item)
			{
				return new ISpecification[]{
					//code must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Code))
						.FailOn(nameof(CollectionPersist.Code)).FailWith(this._localizer["validation_required", nameof(CollectionPersist.Code)]),
					//code max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Code))
						.Must(() => this.LessEqual(item.Code, PersistValidator.CodeMaxLength))
						.FailOn(nameof(CollectionPersist.Code)).FailWith(this._localizer["validation_maxLength", nameof(CollectionPersist.Code)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(CollectionPersist.Name)).FailWith(this._localizer["validation_required", nameof(CollectionPersist.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, PersistValidator.NameMaxLength))
						.FailOn(nameof(CollectionPersist.Name)).FailWith(this._localizer["validation_maxLength", nameof(CollectionPersist.Name)]),
				};
			}
		}
	}

	public class CollectionPersistDeep
	{
		public Guid? Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }
		public List<Guid> Datasets { get; set; }

		public class PersistValidator : BaseValidator<CollectionPersistDeep>
		{
			private static int CodeMaxLength = typeof(Service.DataManagement.Data.Collection).MaxLengthOf(nameof(Service.DataManagement.Data.Collection.Code));
			private static int NameMaxLength = typeof(Service.DataManagement.Data.Collection).MaxLengthOf(nameof(Service.DataManagement.Data.Collection.Name));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(CollectionPersistDeep item)
			{
				return new ISpecification[]{
					//code must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Code))
						.FailOn(nameof(CollectionPersistDeep.Code)).FailWith(this._localizer["validation_required", nameof(CollectionPersistDeep.Code)]),
					//code max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Code))
						.Must(() => this.LessEqual(item.Code, PersistValidator.CodeMaxLength))
						.FailOn(nameof(CollectionPersistDeep.Code)).FailWith(this._localizer["validation_maxLength", nameof(CollectionPersistDeep.Code)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(CollectionPersistDeep.Name)).FailWith(this._localizer["validation_required", nameof(CollectionPersistDeep.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, PersistValidator.NameMaxLength))
						.FailOn(nameof(CollectionPersistDeep.Name)).FailWith(this._localizer["validation_maxLength", nameof(CollectionPersistDeep.Name)]),
				};
			}
		}
	}

	public class CollectionDatasetPatch
	{
		public Guid? Id { get; set; }
		public List<Guid> Datasets { get; set; }

		public class PatchValidator : BaseValidator<CollectionDatasetPatch>
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

			protected override IEnumerable<ISpecification> Specifications(CollectionDatasetPatch item)
			{
				return new ISpecification[]{
					//id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(CollectionDatasetPatch.Id)).FailWith(this._localizer["validation_required", nameof(CollectionDatasetPatch.Id)]),
				};
			}
		}
	}
}
