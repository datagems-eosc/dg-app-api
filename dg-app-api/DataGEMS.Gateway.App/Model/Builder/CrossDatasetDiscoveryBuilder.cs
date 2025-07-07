using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Service.Discovery;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Data;
using DataGEMS.Gateway.App.Query;
using Newtonsoft.Json.Linq;

namespace DataGEMS.Gateway.App.Model.Builder
{
    public class CrossDatasetDiscoveryBuilder : Builder<DataGEMS.Gateway.App.Model.CrossDatasetDiscovery, DataGEMS.Gateway.App.Service.Discovery.Model.CrossDatasetDiscoveryResult>
	{
		private readonly CrossDatasetDiscoveryHttpConfig _httpConfig;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public CrossDatasetDiscoveryBuilder(
			CrossDatasetDiscoveryHttpConfig httpConfig,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<CrossDatasetDiscoveryBuilder> logger) : base(logger)
		{
			this._httpConfig = httpConfig;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public CrossDatasetDiscoveryBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }


		public override async Task<List<CrossDatasetDiscovery>> Build(IFieldSet fields, IEnumerable<Service.Discovery.Model.CrossDatasetDiscoveryResult> datas)
		{
			// TODO: logger
			/*this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.UserCollection)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<UserCollection>().ToList();*/

			Dictionary<String, Guid> sourceIdToDatasetIdMap = this.MapSourceIdToDatasetId(datas);

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(CrossDatasetDiscovery.Dataset)));
			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(fields, sourceIdToDatasetIdMap.Values);

			// Mapping source id to datasets fropm DB
			/*datasetMap = await SourceIdToDatasetMap(datas, fields);

			List<CrossDatasetDiscovery> models = datas.Select(r => new CrossDatasetDiscovery
			{
				Content = r.Content,
				UseCase = r.UseCase,
				Dataset = datasetMap.TryGetValue(r.SourceId, out var dataset) ? dataset : null,
				ChunkId = r.ChunkId,
				Language = r.Language,
				Distance = r.Distance
			}).ToList();

			return models;*/
			return new List<CrossDatasetDiscovery>();
		}
			
		private Dictionary<String, Guid> MapSourceIdToDatasetId(IEnumerable<DataGEMS.Gateway.App.Service.Discovery.Model.CrossDatasetDiscoveryResult> items)
		{
			HashSet<String> includedSourceIds = items.Where(x => !String.IsNullOrEmpty(x.SourceId)).Select(x => x.SourceId).Distinct().ToHashSet();
 			Dictionary<String, Guid> sourceIdMapping = this._httpConfig.SourceIdMapping.Where(x=> includedSourceIds.Contains(x.SourceId)).ToDictionary(x => x.SourceId, x => x.DatasetId);
			return sourceIdMapping;
		}


		/*private async Task<Dictionary<string, Dataset>> SourceIdToDatasetMap(IEnumerable<CrossDatasetDiscoveryResult> raw, IFieldSet fieldset)
		{
			if (raw == null || !raw.Any()) return [];

			// Collect all SourceIds
			HashSet<string> sourceIds = raw
				.Select(r => r.SourceId)
				.Where(id => !string.IsNullOrWhiteSpace(id))
				.ToHashSet();

			// sourceIds intersection SourceIdMapping from config
			List<SourceIdMap> matchedMappings = _httpConfig.SourceIdMapping
				.Where(m => sourceIds.Contains(m.SourceId))
				.ToList();

			if (!matchedMappings.Any()) return [];

			// Collect all datasetIds and collect all datasets
			IEnumerable<Guid> datasetIds = matchedMappings.Select(m => m.DatasetId).Distinct();
			
			// Collect the datasets
			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(fieldset, datasetIds);

			// Connect sourceIds to Datasets
			Dictionary<string, Dataset> result = new();
			foreach (SourceIdMap mapping in matchedMappings)
			{
				if (datasetMap.TryGetValue(mapping.DatasetId, out Dataset dataset))
				{
					result[mapping.SourceId] = dataset;
				}
			}

			return result;
		}*/


		private async Task<Dictionary<Guid, Dataset>> CollectDatasets(IFieldSet fields, IEnumerable<Guid> datasetIds)
		{
			if (fields.IsEmpty() || !datasetIds.Any()) return null;

			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Dataset)).And("fields", fields).And("datasetCount", datasetIds?.Count()));

			Dictionary<Guid, Dataset> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Dataset.Id)))) itemMap = this.AsEmpty(datasetIds.Distinct(), id => new Dataset { Id = id }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Dataset.Id));

				List<DataManagement.Model.Dataset> models = await this._queryFactory
					.Query<DatasetLocalQuery>()
					.DisableTracking()
					.Ids(datasetIds.Distinct())
					.Authorize(this._authorize)
					.CollectAsyncAsModels();

				itemMap = await this._builderFactory
					.Builder<DatasetBuilder>()
					.Authorize(this._authorize)
					.AsForeignKey(models, clone, x => x.Id.Value);
			}

			if (!fields.HasField(nameof(Dataset.Id)))
				itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}
	}
}
