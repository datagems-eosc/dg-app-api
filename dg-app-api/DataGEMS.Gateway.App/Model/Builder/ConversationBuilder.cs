using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Query;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class ConversationBuilder : Builder<Conversation, Data.Conversation>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public ConversationBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<ConversationBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public ConversationBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<Conversation>> Build(IFieldSet fields, IEnumerable<Data.Conversation> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.Conversation)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<Conversation>().ToList();

			IFieldSet conversationDatasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(Conversation.ConversationDatasets)));
			Dictionary<Guid, List<ConversationDataset>> conversationDatasetMap = await this.CollectConversationDatasets(conversationDatasetFields, datas);

			IFieldSet userFields = fields.ExtractPrefixed(this.AsPrefix(nameof(Conversation.User)));
			Dictionary<Guid, User> userMap = await this.CollectUsers(userFields, datas);

			IFieldSet conversationMessageFields = fields.ExtractPrefixed(this.AsPrefix(nameof(Conversation.ConversationMessages)));
			Dictionary<Guid, List<ConversationMessage>> conversationMessageMap = await this.CollectConversationMessages(conversationMessageFields, datas);

			List<Conversation> models = new List<Conversation>();
			foreach (Data.Conversation d in datas ?? new List<Data.Conversation>())
			{
				Conversation m = new Conversation();
				if (fields.HasField(nameof(Conversation.ETag))) m.ETag = d.UpdatedAt.ToETag();
				if (fields.HasField(nameof(Conversation.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(Conversation.Name))) m.Name = d.Name;
				if (fields.HasField(nameof(Conversation.IsActive))) m.IsActive = d.IsActive;
				if (fields.HasField(nameof(Conversation.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(Conversation.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!conversationDatasetFields.IsEmpty() && conversationDatasetMap != null && conversationDatasetMap.ContainsKey(d.Id)) m.ConversationDatasets = conversationDatasetMap[d.Id];
				if (!userFields.IsEmpty() && userMap != null && userMap.ContainsKey(d.UserId)) m.User = userMap[d.UserId];
				if (!conversationMessageFields.IsEmpty() && conversationMessageMap != null && conversationMessageMap.ContainsKey(d.Id)) m.ConversationMessages = conversationMessageMap[d.Id];

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, List<ConversationDataset>>> CollectConversationDatasets(IFieldSet fields, IEnumerable<Data.Conversation> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.ConversationDataset)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, List<ConversationDataset>> itemMap = null;
			IFieldSet clone = new FieldSet(fields.Fields).Ensure(this.AsIndexer(nameof(ConversationDataset.Conversation), nameof(Conversation.Id)));
			ConversationDatasetQuery query = this._queryFactory.Query<ConversationDatasetQuery>().DisableTracking().IsActive(Common.IsActive.Active).ConversationIds(datas.Select(x => x.Id).Distinct()).Authorize(this._authorize);
			itemMap = await this._builderFactory.Builder<ConversationDatasetBuilder>().Authorize(this._authorize).AsMasterKey(query, clone, x => x.Conversation.Id.Value);

			if (!fields.HasField(this.AsIndexer(nameof(ConversationDataset.Conversation), nameof(Conversation.Id)))) itemMap.SelectMany(x => x.Value).Where(x => x != null && x.Conversation != null).ToList().ForEach(x => x.Conversation.Id = null);

			return itemMap;
		}

		private async Task<Dictionary<Guid, User>> CollectUsers(IFieldSet fields, IEnumerable<Data.Conversation> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.User)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, User> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(User.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.UserId).Distinct(), x => new User() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(User.Id));
				UserQuery query = this._queryFactory.Query<UserQuery>().DisableTracking().Ids(datas.Select(x => x.UserId).Distinct()).Authorize(this._authorize);
				itemMap = await this._builderFactory.Builder<UserBuilder>().Authorize(this._authorize).AsForeignKey(query, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(User.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}

		private async Task<Dictionary<Guid, List<ConversationMessage>>> CollectConversationMessages(IFieldSet fields, IEnumerable<Data.Conversation> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.ConversationMessage)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, List<ConversationMessage>> itemMap = null;
			IFieldSet clone = new FieldSet(fields.Fields).Ensure(this.AsIndexer(nameof(ConversationMessage.Conversation), nameof(Conversation.Id)));
			ConversationMessageQuery query = this._queryFactory.Query<ConversationMessageQuery>().DisableTracking().ConversationIds(datas.Select(x => x.Id).Distinct()).Authorize(this._authorize);
			itemMap = await this._builderFactory.Builder<ConversationMessageBuilder>().Authorize(this._authorize).AsMasterKey(query, clone, x => x.Conversation.Id.Value);

			if (!fields.HasField(this.AsIndexer(nameof(ConversationMessage.Conversation), nameof(Conversation.Id)))) itemMap.SelectMany(x => x.Value).Where(x => x != null && x.Conversation != null).ToList().ForEach(x => x.Conversation.Id = null);

			return itemMap;
		}
	}
}
