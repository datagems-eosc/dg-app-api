using Cite.Tools.Common.Extensions;
using Cite.Tools.Validation;
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
	public class WorkflowExecutionArgs
	{
		public String WorkflowId { get; set; }

		public class WorkflowExecutionArgsValidator : BaseValidator<WorkflowExecutionArgs>
		{
			private static int NameMaxLength = typeof(Data.Conversation).MaxLengthOf(nameof(Data.Conversation.Name));

			public WorkflowExecutionArgsValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<WorkflowExecutionArgsValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(WorkflowExecutionArgs item)
			{
				return new ISpecification[] {
					//workflow id must always be set
					this.Spec()
						.Must(() => !this.IsEmpty(item.WorkflowId))
						.FailOn(nameof(WorkflowExecutionArgs.WorkflowId)).FailWith(this._localizer["validation_required", nameof(WorkflowExecutionArgs.WorkflowId)]),
				};
			}
		}
	}
}
