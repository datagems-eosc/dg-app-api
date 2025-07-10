using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Query;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class ConversationMessageBuilder : Builder<ConversationMessage, Data.ConversationMessage>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public ConversationMessageBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<ConversationMessageBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public ConversationMessageBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<ConversationMessage>> Build(IFieldSet fields, IEnumerable<Data.ConversationMessage> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.ConversationMessage)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<ConversationMessage>().ToList();

			IFieldSet conversationFields = fields.ExtractPrefixed(this.AsPrefix(nameof(ConversationMessage.Conversation)));
			Dictionary<Guid, Conversation> conversationMap = await this.CollectConversations(conversationFields, datas);

			List<ConversationMessage> models = new List<ConversationMessage>();
			foreach (Data.ConversationMessage d in datas ?? new List<Data.ConversationMessage>())
			{
				ConversationMessage m = new ConversationMessage();
				if (fields.HasField(nameof(ConversationMessage.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(ConversationMessage.Kind))) m.Kind = d.Kind;
				if (fields.HasField(nameof(ConversationMessage.Data))) m.Data = d.Data;
				if (fields.HasField(nameof(ConversationMessage.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (!conversationFields.IsEmpty() && conversationMap != null && conversationMap.ContainsKey(d.ConversationId)) m.Conversation = conversationMap[d.ConversationId];

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, Conversation>> CollectConversations(IFieldSet fields, IEnumerable<Data.ConversationMessage> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Conversation)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, Conversation> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Conversation.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.ConversationId).Distinct(), x => new Conversation() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Conversation.Id));
				ConversationQuery query = this._queryFactory.Query<ConversationQuery>().DisableTracking().Ids(datas.Select(x => x.ConversationId).Distinct()).Authorize(this._authorize);
				itemMap = await this._builderFactory.Builder<ConversationBuilder>().Authorize(this._authorize).AsForeignKey(query, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(Conversation.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}

	}
}
