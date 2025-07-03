using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.Common;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model
{
	public class Conversation
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public User User { get; set; }
		public List<ConversationDataset> ConversationDatasets { get; set; }
		public List<ConversationMessage> ConversationMessages { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public String ETag { get; set; }
	}

	public class ConversationPersist
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public String ETag { set; get; }

		public class PersistValidator : BaseValidator<ConversationPersist>
		{
			private static int NameMaxLength = typeof(Data.Conversation).MaxLengthOf(nameof(Data.Conversation.Name));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(ConversationPersist item)
			{
				return new ISpecification[] {
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(ConversationPersist.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(ConversationPersist.ETag)).FailWith(this._localizer["validation_required", nameof(ConversationPersist.ETag)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(ConversationPersist.Name)).FailWith(this._localizer["validation_required", nameof(ConversationPersist.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, PersistValidator.NameMaxLength))
						.FailOn(nameof(ConversationPersist.Name)).FailWith(this._localizer["validation_maxLength", nameof(ConversationPersist.Name)]),
				};
			}
		}
	}

	public class ConversationPersistDeep
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public List<ConversationDatasetPersist> ConversationDatasets { get; set; }
		public String ETag { get; set; }

		public class PersistValidator : BaseValidator<ConversationPersistDeep>
		{
			private static int NameMaxLength = typeof(Data.Conversation).MaxLengthOf(nameof(Data.Conversation.Name));

			public PersistValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<PersistValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(ConversationPersistDeep item)
			{
				return new ISpecification[]{
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(ConversationPersistDeep.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(ConversationPersistDeep.ETag)).FailWith(this._localizer["validation_required", nameof(ConversationPersistDeep.ETag)]),
					//name must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.Name))
						.FailOn(nameof(ConversationPersistDeep.Name)).FailWith(this._localizer["validation_required", nameof(ConversationPersistDeep.Name)]),
					//name max length
					this.Spec()
						.If(() => !this.IsEmpty(item.Name))
						.Must(() => this.LessEqual(item.Name, PersistValidator.NameMaxLength))
						.FailOn(nameof(ConversationPersistDeep.Name)).FailWith(this._localizer["validation_maxLength", nameof(ConversationPersistDeep.Name)]),
					//conversation dataset must be valid if set
					this.NavSpec()
						.If(() => item.ConversationDatasets != null)
						.On(nameof(ConversationPersistDeep.ConversationDatasets))
						.Over(item.ConversationDatasets)
						.Using(()=>_validatorFactory[typeof(ConversationDatasetPersist.PersistSubDeepValidator)]),
				};
			}
		}
	}


	public class ConversationDatasetPatch
	{
		public Guid? Id { get; set; }
		public List<ConversationDatasetPersist> ConversationDatasets { get; set; }
		public String ETag { get; set; }

		public class PatchValidator : BaseValidator<ConversationDatasetPatch>
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

			protected override IEnumerable<ISpecification> Specifications(ConversationDatasetPatch item)
			{
				return new ISpecification[]{
					//id must always be set
					this.Spec()
						.Must(() => this.IsValidGuid(item.Id))
						.FailOn(nameof(ConversationDatasetPatch.Id)).FailWith(this._localizer["validation_required", nameof(ConversationDatasetPatch.Id)]),
					//hash must always be set
					this.Spec()
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(ConversationDatasetPatch.ETag)).FailWith(this._localizer["validation_required", nameof(ConversationDatasetPatch.ETag)]),
					//attributes must be valid if set
					this.NavSpec()
						.If(() => item.ConversationDatasets != null)
						.On(nameof(ConversationDatasetPatch.ConversationDatasets))
						.Over(item.ConversationDatasets)
						.Using(()=>this._validatorFactory[typeof(ConversationDatasetPersist.PersistSubDeepValidator)]),
				};
			}
		}
	}
}
