using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class AdHocQueryBuilder : Builder<App.Model.AdHocQuery, Data.AdHocQueryResult>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public AdHocQueryBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<AdHocQueryBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public AdHocQueryBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<AdHocQuery>> Build(IFieldSet fields, IEnumerable<Data.AdHocQueryResult> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.AdHocQuery)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<AdHocQuery>().ToList();

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(AdHocQuery.Dataset)));
			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(datasetFields, datas);

			IFieldSet userFields = fields.ExtractPrefixed(this.AsPrefix(nameof(AdHocQuery.User)));
			Dictionary<Guid, User> userMap = await this.CollectUsers(userFields, datas);

			List<AdHocQuery> models = [];
			foreach (Data.AdHocQueryResult d in datas ?? new List<Data.AdHocQueryResult>())
			{
				AdHocQuery m = new AdHocQuery();
				if (fields.HasField(nameof(AdHocQuery.Id))) m.Id = d.Id;
				if(fields.HasField(nameof(AdHocQuery.AnalyticalPattern))) m.AnalyticalPattern = d.AnalyticalPattern;
				if (fields.HasField(nameof(AdHocQuery.IsActive))) m.IsActive = d.IsActive;
				if (fields.HasField(nameof(AdHocQuery.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(AdHocQuery.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!datasetFields.IsEmpty() && datasetMap != null && datasetMap.ContainsKey(d.DatasetId)) m.Dataset = datasetMap[d.DatasetId];
				if (!userFields.IsEmpty() && userMap != null && userMap.ContainsKey(d.UserId)) m.User = userMap[d.UserId];

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, Dataset>> CollectDatasets(IFieldSet fields, IEnumerable<Data.AdHocQueryResult> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Dataset)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, Dataset> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Dataset.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.DatasetId).Distinct(), x => new Dataset() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Dataset.Id));
				List<Service.DataManagement.Model.Dataset> models = (await this._queryFactory.Query<DatasetHttpQuery>().Ids(datas.Select(x => x.DatasetId).Distinct()).CollectAsync())?.Items ?? [];
				itemMap = await this._builderFactory.Builder<DatasetBuilder>().Authorize(this._authorize).AsForeignKey(models, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(Dataset.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}

		private async Task<Dictionary<Guid, User>> CollectUsers(IFieldSet fields, IEnumerable<Data.AdHocQueryResult> datas)
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
	}
}
