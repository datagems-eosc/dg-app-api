using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class ConversationDataset
	{
		public Guid? Id { get; set; }
		public Dataset Dataset { get; set; }
		public Conversation Conversation { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public String ETag { get; set; }
	}

	public class ConversationDatasetPersist
	{
		public Guid? Id { get; set; }
		public Guid? ConversationId { get; set; }
		public Guid? DatasetId { get; set; }
		public String ETag { get; set; }

		public class PersistValidator : BaseValidator<ConversationDatasetPersist>
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

			protected override IEnumerable<ISpecification> Specifications(ConversationDatasetPersist item)
			{
				return new ISpecification[]{
					//creating new item. Hash must not be set
					this.Spec()
						.If(() => !this.IsValidGuid(item.Id))
						.Must(() => !this.IsValidHash(item.ETag))
						.FailOn(nameof(ConversationDatasetPersist.ETag)).FailWith(this._localizer["validation_overPosting"]),
					//update existing item. Hash must be set
					this.Spec()
						.If(() => this.IsValidGuid(item.Id))
						.Must(() => this.IsValidHash(item.ETag))
						.FailOn(nameof(ConversationDatasetPersist.ETag)).FailWith(this._localizer["validation_required", nameof(ConversationDatasetPersist.ETag)]),
					//conversation must always be set
					this.Spec()
						.Must(() => !this.IsValidGuid(item.ConversationId))
						.FailOn(nameof(ConversationDatasetPersist.ConversationId)).FailWith(this._localizer["validation_required", nameof(ConversationDatasetPersist.ConversationId)]),
					//dataset must always be set
					this.Spec()
						.Must(() => !this.IsValidGuid(item.DatasetId))
						.FailOn(nameof(ConversationDatasetPersist.DatasetId)).FailWith(this._localizer["validation_required", nameof(ConversationDatasetPersist.DatasetId)]),
				};
			}
		}
	}
}
