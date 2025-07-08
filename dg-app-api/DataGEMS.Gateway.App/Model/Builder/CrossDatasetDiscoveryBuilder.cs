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
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

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
			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(datasetFields, datas);

			List<CrossDatasetDiscovery> models = new List<CrossDatasetDiscovery>();
			foreach (Service.Discovery.Model.CrossDatasetDiscoveryResult d in datas ?? new List<Service.Discovery.Model.CrossDatasetDiscoveryResult>())
			{
				CrossDatasetDiscovery m = new CrossDatasetDiscovery();
				if (fields.HasField(nameof(CrossDatasetDiscovery.Content))) m.Content = d.Content;
				if (fields.HasField(nameof(CrossDatasetDiscovery.UseCase))) m.UseCase = d.UseCase;
				if (fields.HasField(nameof(CrossDatasetDiscovery.SourceId))) m.SourceId = d.SourceId;
				if (fields.HasField(nameof(CrossDatasetDiscovery.ChunkId))) m.ChunkId = d.ChunkId;
				if (fields.HasField(nameof(CrossDatasetDiscovery.Language))) m.Language = d.Language;
				if (fields.HasField(nameof(CrossDatasetDiscovery.Distance))) m.Distance = d.Distance;
				if (!datasetFields.IsEmpty() && datasetMap != null && datasetMap.ContainsKey(d.Source)) m.Dataset = datasetMap[d.Source];

				models.Add(m);
			}
			return models;
		}


		private async Task<Dictionary<Guid, Dataset>> CollectDatasets(IFieldSet fields, IEnumerable<Service.Discovery.Model.CrossDatasetDiscoveryResult> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Dataset)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, Dataset> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Dataset.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.Source).Distinct(), x => new Dataset() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Dataset.Id));
				List<DataManagement.Model.Dataset> models = await this._queryFactory.Query<DatasetLocalQuery>().DisableTracking().Ids(datas.Select(x => x.Source).Distinct()).Authorize(this._authorize).CollectAsyncAsModels();
				itemMap = await this._builderFactory.Builder<DatasetBuilder>().Authorize(this._authorize).AsForeignKey(models, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(Dataset.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}
	}
}
