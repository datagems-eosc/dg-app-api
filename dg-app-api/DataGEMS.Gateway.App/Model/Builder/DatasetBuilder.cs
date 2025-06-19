using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class DatasetBuilder : PrimitiveBuilder<Model.Dataset, DataManagement.Model.Dataset>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public DatasetBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<DatasetBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public DatasetBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<Model.Dataset>> Build(IFieldSet fields, IEnumerable<DataManagement.Model.Dataset> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Model.Dataset)).And("fields", fields).And("data", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<Model.Dataset>().ToList();

			IFieldSet collectionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(Model.Dataset.Collections)));
			Dictionary<Guid, List<Model.Collection>> collectionMap = await this.CollectCollections(collectionFields, datas);

			IFieldSet permissionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(Model.Dataset.Permissions)));
			Dictionary<Guid, HashSet<String>> datasetAffiliatedRoles = null;
			if (!permissionFields.IsEmpty()) datasetAffiliatedRoles = await this._authorizationContentResolver.DatasetRolesForDataset(datas.Select(x=> x.Id).Distinct().ToList());

			List<Model.Dataset> models = new List<Model.Dataset>();
			foreach(DataManagement.Model.Dataset d in datas ?? Enumerable.Empty<DataManagement.Model.Dataset>())
			{
				Model.Dataset m = new Model.Dataset();
				if (fields.HasField(nameof(Model.Dataset.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(Model.Dataset.Code))) m.Code = d.Code;
				if (fields.HasField(nameof(Model.Dataset.Name))) m.Name = d.Name;
				if (!collectionFields.IsEmpty() && collectionMap != null && collectionMap.ContainsKey(d.Id)) m.Collections = collectionMap[d.Id];
				if (!permissionFields.IsEmpty() && datasetAffiliatedRoles != null && datasetAffiliatedRoles.ContainsKey(d.Id))
				{
					ISet<String> affiliatedPermissions = this._authorizationContentResolver.PermissionsOfDatasetRoles(datasetAffiliatedRoles[d.Id]);
					m.Permissions = permissionFields.Fields.ReduceToAssignedPermissions(affiliatedPermissions);
				}

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, List<Model.Collection>>> CollectCollections(IFieldSet fields, IEnumerable<DataManagement.Model.Dataset> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("collecting").And("type", nameof(Model.Collection)).And("fields", fields).And("data", datas?.Count()));

			List<DataManagement.Model.DatasetCollection> datasetCollections = await this._queryFactory.Query<DatasetCollectionLocalQuery>().DatasetIds(datas.Select(x => x.Id).Distinct().ToList()).DisableTracking().Authorize(this._authorize).CollectAsyncAsModels();
			List<Guid> collectionIds = datasetCollections.Select(x=> x.CollectionId).Distinct().ToList();
			Dictionary<Guid, List<Guid>> collectionsOfDataset = datasetCollections.ToDictionaryOfList(x => x.DatasetId).ToDictionary(x => x.Key, x => x.Value.Select(y => y.CollectionId).ToList());

			List<DataManagement.Model.Collection> collections = await this._queryFactory.Query<CollectionLocalQuery>().Ids(collectionIds).DisableTracking().Authorize(this._authorize).CollectAsyncAsModels();
			IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Model.Collection.Id));
			List<Model.Collection> collectionModels = await this._builderFactory.Builder<CollectionBuilder>().Authorize(this._authorize).Build(clone, collections);
			Dictionary<Guid, Model.Collection> collectionMap = collectionModels.ToDictionary(x => x.Id.Value);

			Dictionary<Guid, List<Model.Collection>> itemMap = new Dictionary<Guid, List<Collection>>();
			foreach (KeyValuePair<Guid,List<Guid>> collectionsOfDatasetPair in collectionsOfDataset)
			{
				List<Model.Collection> colls = collectionsOfDatasetPair.Value.Where(x => collectionMap.ContainsKey(x)).Select(x => collectionMap[x]).ToList();
				itemMap.Add(collectionsOfDatasetPair.Key, colls);
			}

			if (!fields.HasField(nameof(Model.Collection.Id))) itemMap.SelectMany(x => x.Value).Where(x => x != null && x.Id.HasValue).ToList().ForEach(x => x.Id = null);

			return itemMap;

		}
	}
}

