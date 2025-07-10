using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class UserDatasetCollectionBuilder : Builder<UserDatasetCollection, Data.UserDatasetCollection>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserDatasetCollectionBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<UserDatasetCollectionBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public UserDatasetCollectionBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<UserDatasetCollection>> Build(IFieldSet fields, IEnumerable<Data.UserDatasetCollection> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.UserDatasetCollection)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<UserDatasetCollection>().ToList();

			IFieldSet userCollectionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(UserDatasetCollection.UserCollection)));
			Dictionary<Guid, UserCollection> userCollectionMap = await this.CollectUserCollections(userCollectionFields, datas);

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(UserDatasetCollection.Dataset)));
			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(datasetFields, datas);

			List<UserDatasetCollection> models = new List<UserDatasetCollection>();
			foreach (Data.UserDatasetCollection d in datas ?? new List<Data.UserDatasetCollection>())
			{
				UserDatasetCollection m = new UserDatasetCollection();
				if (fields.HasField(nameof(UserDatasetCollection.ETag))) m.ETag = d.UpdatedAt.ToETag();
				if (fields.HasField(nameof(UserDatasetCollection.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(UserDatasetCollection.IsActive))) m.IsActive = d.IsActive;
				if (fields.HasField(nameof(UserDatasetCollection.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(UserDatasetCollection.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!userCollectionFields.IsEmpty() && userCollectionMap != null && userCollectionMap.ContainsKey(d.UserCollectionId)) m.UserCollection = userCollectionMap[d.UserCollectionId];
				if (!datasetFields.IsEmpty() && datasetMap != null && datasetMap.ContainsKey(d.DatasetId)) m.Dataset = datasetMap[d.DatasetId];

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, UserCollection>> CollectUserCollections(IFieldSet fields, IEnumerable<Data.UserDatasetCollection> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.UserCollection)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, UserCollection> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(UserCollection.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.UserCollectionId).Distinct(), x => new UserCollection() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(UserCollection.Id));
				UserCollectionQuery q = this._queryFactory.Query<UserCollectionQuery>().DisableTracking().Ids(datas.Select(x => x.UserCollectionId).Distinct()).Authorize(this._authorize);
				itemMap = await this._builderFactory.Builder<UserCollectionBuilder>().Authorize(this._authorize).AsForeignKey(q, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(UserCollection.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}

		private async Task<Dictionary<Guid, Dataset>> CollectDatasets(IFieldSet fields, IEnumerable<Data.UserDatasetCollection> datas)
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
