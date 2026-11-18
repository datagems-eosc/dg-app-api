using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Query;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class CrossDatasetDiscoveryBuilder : Builder<CrossDatasetDiscovery, Service.Discovery.Model.CrossDatasetDiscoveryResult>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public CrossDatasetDiscoveryBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<CrossDatasetDiscoveryBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public CrossDatasetDiscoveryBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<CrossDatasetDiscovery>> Build(IFieldSet fields, IEnumerable<Service.Discovery.Model.CrossDatasetDiscoveryResult> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Discovery.Model.CrossDatasetDiscoveryResult)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<CrossDatasetDiscovery>().ToList();

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(CrossDatasetDiscovery.Dataset)));
			List<Service.DataManagement.Model.Dataset> accessibleDatasets = await this.CollectAccessibleDatasets(datasetFields, datas);
			HashSet<Guid> accessibleDatasetIds = accessibleDatasets.Select(x => x.Id).ToHashSet();
			List<Service.Discovery.Model.CrossDatasetDiscoveryResult> accessibleDatas = (datas ?? []).Where(x => accessibleDatasetIds.Contains(x.DatasetId)).ToList();
			int droppedCount = (datas?.Count() ?? 0) - accessibleDatas.Count;
			if (droppedCount > 0) this._logger.Debug(new MapLogEntry("dropping results of datasets that could not be loaded").And("droppedCount", droppedCount));

			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(datasetFields, accessibleDatasets);

			IFieldSet hitsFields = fields.ExtractPrefixed(this.AsPrefix(nameof(CrossDatasetDiscovery.Hits)));

			Dictionary<Guid, OrderableResult> byDataset = this.GroupResultsByDataset(accessibleDatas);

			List<CrossDatasetDiscovery> models = new List<CrossDatasetDiscovery>();
			foreach (KeyValuePair<Guid, OrderableResult> dpair in byDataset.OrderBy(x=> x.Value.ResultOrdinal))
			{
				if (!datasetFields.IsEmpty() && (datasetMap == null || !datasetMap.ContainsKey(dpair.Key) || datasetMap[dpair.Key] == null)) continue;

				CrossDatasetDiscovery m = new CrossDatasetDiscovery();

				if (!datasetFields.IsEmpty() && datasetMap != null && datasetMap.ContainsKey(dpair.Key)) m.Dataset = datasetMap[dpair.Key];
				if (fields.HasField(nameof(CrossDatasetDiscovery.MaxSimilarity)) && dpair.Value.Count > 0) m.MaxSimilarity = dpair.Value.Max(x => x.Similarity);

				if (!hitsFields.IsEmpty() && dpair.Value.Count > 0)
				{
					m.Hits = new List<CrossDatasetDiscovery.DatasetHits>();
					foreach (Service.Discovery.Model.CrossDatasetDiscoveryResult d in dpair.Value)
					{
						CrossDatasetDiscovery.DatasetHits h = new CrossDatasetDiscovery.DatasetHits();
						if (hitsFields.HasField(nameof(CrossDatasetDiscovery.DatasetHits.Content))) h.Content = d.Content;
						if (hitsFields.HasField(nameof(CrossDatasetDiscovery.DatasetHits.ObjectId))) h.ObjectId = d.ObjectId;
						if (hitsFields.HasField(nameof(CrossDatasetDiscovery.DatasetHits.Similarity))) h.Similarity = d.Similarity;
						m.Hits.Add(h);
					}
				}

				models.Add(m);
			}

			return models;
		}

		private Dictionary<Guid, OrderableResult> GroupResultsByDataset(IEnumerable<Service.Discovery.Model.CrossDatasetDiscoveryResult> datas)
		{
			Dictionary<Guid, OrderableResult> byDataset = new Dictionary<Guid, OrderableResult>();
			int counter = 0;
			foreach (Service.Discovery.Model.CrossDatasetDiscoveryResult item in datas ?? Enumerable.Empty<Service.Discovery.Model.CrossDatasetDiscoveryResult>())
			{
				if (!byDataset.ContainsKey(item.DatasetId))
				{
					byDataset.Add(item.DatasetId, new OrderableResult() { ResultOrdinal = counter });
					counter += 1;
				}

				byDataset[item.DatasetId].Add(item);
			}
			return byDataset;
		}

		private class OrderableResult : List<Service.Discovery.Model.CrossDatasetDiscoveryResult>
		{
			public int ResultOrdinal { get; set; }
		}

		private async Task<List<Service.DataManagement.Model.Dataset>> CollectAccessibleDatasets(IFieldSet datasetFields, IEnumerable<Service.Discovery.Model.CrossDatasetDiscoveryResult> datas)
		{
			List<Guid> datasetIds = (datas ?? []).Select(x => x.DatasetId).Distinct().ToList();
			if (datasetIds.Count == 0) return [];

			IFieldSet projection = datasetFields.HasOtherField(this.AsIndexer(nameof(Dataset.Id))) ? null : new FieldSet(nameof(Dataset.Id));
			return (await this._queryFactory.Query<DatasetHttpQuery>().Ids(datasetIds).CollectAsync(projection))?.Items ?? [];
		}

		private async Task<Dictionary<Guid, Dataset>> CollectDatasets(IFieldSet fields, List<Service.DataManagement.Model.Dataset> models)
		{
			if (fields.IsEmpty() || models.Count == 0) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Dataset)).And("fields", fields).And("dataCount", models.Count));

			Dictionary<Guid, Dataset> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Dataset.Id)))) itemMap = this.AsEmpty(models.Select(x => x.Id).Distinct(), x => new Dataset() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Dataset.Id));
				itemMap = await this._builderFactory.Builder<DatasetBuilder>().Authorize(this._authorize).AsForeignKey(models, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(Dataset.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}
	}
}
