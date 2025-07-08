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
	public class ConversationDatasetBuilder : Builder<ConversationDataset, Data.ConversationDataset>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public ConversationDatasetBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<ConversationDatasetBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public ConversationDatasetBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<ConversationDataset>> Build(IFieldSet fields, IEnumerable<Data.ConversationDataset> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.ConversationDataset)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<ConversationDataset>().ToList();

			IFieldSet conversationFields = fields.ExtractPrefixed(this.AsPrefix(nameof(ConversationDataset.Conversation)));
			Dictionary<Guid, Conversation> conversationMap = await this.CollectConversations(conversationFields, datas);

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(ConversationDataset.Dataset)));
			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(datasetFields, datas);

			List<ConversationDataset> models = new List<ConversationDataset>();
			foreach (Data.ConversationDataset d in datas ?? new List<Data.ConversationDataset>())
			{
				ConversationDataset m = new ConversationDataset();
				if (fields.HasField(nameof(ConversationDataset.ETag))) m.ETag = d.UpdatedAt.ToETag();
				if (fields.HasField(nameof(ConversationDataset.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(ConversationDataset.IsActive))) m.IsActive = d.IsActive;
				if (fields.HasField(nameof(ConversationDataset.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(ConversationDataset.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!conversationFields.IsEmpty() && conversationMap != null && conversationMap.ContainsKey(d.ConversationId)) m.Conversation = conversationMap[d.ConversationId];
				if (!datasetFields.IsEmpty() && datasetMap != null && datasetMap.ContainsKey(d.DatasetId)) m.Dataset = datasetMap[d.DatasetId];

				models.Add(m);
			}
			return models;
		}


		private async Task<Dictionary<Guid, Conversation>> CollectConversations(IFieldSet fields, IEnumerable<Data.ConversationDataset> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Conversation)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, Conversation> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Conversation.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.ConversationId).Distinct(), x => new Conversation() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Conversation.Id));
				ConversationQuery q = this._queryFactory.Query<ConversationQuery>().DisableTracking().Ids(datas.Select(x => x.ConversationId).Distinct()).Authorize(this._authorize);
				itemMap = await this._builderFactory.Builder<ConversationBuilder>().Authorize(this._authorize).AsForeignKey(q, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(Conversation.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}


		private async Task<Dictionary<Guid, Dataset>> CollectDatasets(IFieldSet fields, IEnumerable<Data.ConversationDataset> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Dataset)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, Dataset> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Dataset.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.DatasetId).Distinct(), x => new Dataset() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Dataset.Id));
				List<DataManagement.Model.Dataset> models = await this._queryFactory.Query<DatasetLocalQuery>().DisableTracking().Ids(datas.Select(x => x.DatasetId).Distinct()).Authorize(this._authorize).CollectAsyncAsModels();
				itemMap = await this._builderFactory.Builder<DatasetBuilder>().Authorize(this._authorize).AsForeignKey(models, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(Dataset.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}
	}
}
