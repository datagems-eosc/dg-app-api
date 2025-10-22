using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.DataManagement.Model;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
    public class CollectionBuilder : PrimitiveBuilder<Model.Collection, Service.DataManagement.Model.Collection>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public CollectionBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<CollectionBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public CollectionBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<Model.Collection>> Build(IFieldSet fields, IEnumerable<Service.DataManagement.Model.Collection> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Model.Collection)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<Model.Collection>().ToList();

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(Model.Collection.Datasets)));
			Dictionary<Guid, List<Model.Dataset>> datasetMap = await this.CollectDatasets(datasetFields, datas);

			IFieldSet permissionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(Model.Collection.Permissions)));
			Dictionary<Guid, HashSet<String>> collectionAffiliatedRoles = null;
			if (!permissionFields.IsEmpty()) collectionAffiliatedRoles = await this._authorizationContentResolver.ContextRolesForCollectionOfUser(datas.Select(x => x.Id).Distinct().ToList());

			List<Model.Collection> models = new List<Model.Collection>();
			foreach(Service.DataManagement.Model.Collection d in datas ?? Enumerable.Empty<Service.DataManagement.Model.Collection>())
			{
				Model.Collection m = new Model.Collection();
				if (fields.HasField(nameof(Model.Collection.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(Model.Collection.Code))) m.Code = d.Code;
				if (fields.HasField(nameof(Model.Collection.Name))) m.Name = d.Name;
				if (fields.HasField(nameof(Model.Collection.DatasetCount))) m.DatasetCount = d.DatasetCount;
				if (!datasetFields.IsEmpty() && datasetMap != null && datasetMap.ContainsKey(d.Id)) m.Datasets = datasetMap[d.Id];
				if (!permissionFields.IsEmpty() && collectionAffiliatedRoles != null && collectionAffiliatedRoles.ContainsKey(d.Id))
				{
					ISet<String> affiliatedPermissions = this._authorizationContentResolver.PermissionsOfContextRoles(collectionAffiliatedRoles[d.Id]);
					m.Permissions = permissionFields.Fields.ReduceToAssignedPermissions(affiliatedPermissions);
				}

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, List<Model.Dataset>>> CollectDatasets(IFieldSet fields, IEnumerable<Service.DataManagement.Model.Collection> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("collecting").And("type", nameof(Model.Dataset)).And("fields", fields).And("data", datas?.Count()));

			List<DatasetCollection> datasetCollections = await this._queryFactory.Query<DatasetCollectionLocalQuery>().CollectionIds(datas.Select(x => x.Id).Distinct().ToList()).DisableTracking().Authorize(this._authorize).CollectAsyncAsModels();
			List<Guid> datasetIds = datasetCollections.Select(x=> x.DatasetId).Distinct().ToList();
			Dictionary<Guid, List<Guid>> datasetsOfCollection = datasetCollections.ToDictionaryOfList(x => x.CollectionId).ToDictionary(x => x.Key, x => x.Value.Select(y => y.DatasetId).ToList());

			List<Service.DataManagement.Model.Dataset> datasets = await this._queryFactory.Query<DatasetLocalQuery>().Ids(datasetIds).DisableTracking().Authorize(this._authorize).CollectAsyncAsModels();
			IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Model.Dataset.Id));
			List<Model.Dataset> datasetModels = await this._builderFactory.Builder<DatasetBuilder>().Authorize(this._authorize).Build(clone, datasets);
			Dictionary<Guid, Model.Dataset> datasetMap = datasetModels.ToDictionary(x => x.Id.Value);

			Dictionary<Guid, List<Model.Dataset>> itemMap = new Dictionary<Guid, List<Dataset>>();
			foreach (KeyValuePair<Guid,List<Guid>> datasetsOfCollectionPair in datasetsOfCollection)
			{
				List<Model.Dataset> dsets = datasetsOfCollectionPair.Value.Where(x => datasetMap.ContainsKey(x)).Select(x => datasetMap[x]).ToList();
				itemMap.Add(datasetsOfCollectionPair.Key, dsets);
			}

			if (!fields.HasField(nameof(Model.Dataset.Id))) itemMap.SelectMany(x => x.Value).Where(x => x != null && x.Id.HasValue).ToList().ForEach(x => x.Id = null);

			return itemMap;

		}
	}
}

