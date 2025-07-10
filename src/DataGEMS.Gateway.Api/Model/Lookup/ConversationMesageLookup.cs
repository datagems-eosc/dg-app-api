using Cite.Tools.Data.Query;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class ConversationMessageLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that have the specific conversation ids. If set, the list of ids must not be empty")]
		public List<Guid> ConversationIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items that have the specific kind. If set, the list of ids must not be empty")]
		public List<ConversationMessageKind> Kinds { get; set; }

		public ConversationMessageQuery Enrich(QueryFactory factory)
		{
			ConversationMessageQuery query = factory.Query<ConversationMessageQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.ConversationIds != null) query.ConversationIds(this.ConversationIds);
			if (this.Kinds != null) query.Kinds(this.Kinds);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<ConversationMessageLookup>
		{
			public QueryValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<QueryValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(ConversationMessageLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(ConversationMessageLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationMessageLookup.Ids)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(ConversationMessageLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationMessageLookup.ExcludedIds)]),
					//conversationIds must be null or not empty
					this.Spec()
						.Must(() => !item.ConversationIds.IsNotNullButEmpty())
						.FailOn(nameof(ConversationMessageLookup.ConversationIds)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationMessageLookup.ConversationIds)]),
					//kinds must be null or not empty
					this.Spec()
						.Must(() => !item.Kinds.IsNotNullButEmpty())
						.FailOn(nameof(ConversationMessageLookup.Kinds)).FailWith(this._localizer["validation_setButEmpty", nameof(ConversationMessageLookup.Kinds)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(ConversationMessageLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
