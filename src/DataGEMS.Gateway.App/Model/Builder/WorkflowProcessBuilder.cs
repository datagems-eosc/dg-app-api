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
	public class WorkflowProcessBuilder : Builder<WorkflowProcess, Data.WorkflowProcess>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowProcessBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<WorkflowProcessBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public WorkflowProcessBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<WorkflowProcess>> Build(IFieldSet fields, IEnumerable<Data.WorkflowProcess> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.WorkflowProcess)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<WorkflowProcess>().ToList();

			IFieldSet processStepFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowProcess.Steps)));
			Dictionary<Guid, List<WorkflowProcessStep>> workflowProcessStepMap = await this.WorkflowProcessSteps(processStepFields, datas);

			IFieldSet userFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowProcess.User)));
			Dictionary<Guid, User> userMap = await this.CollectUsers(userFields, datas);

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowProcess.Dataset)));
			Dictionary<Guid, Dataset> datasetMap = await this.CollectDatasets(datasetFields, datas);

			List<WorkflowProcess> models = [];
			foreach (Data.WorkflowProcess d in datas ?? [])
			{
				WorkflowProcess m = new WorkflowProcess();
				if (fields.HasField(nameof(WorkflowProcess.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(WorkflowProcess.ProcessId))) m.ProcessId = d.ProcessId;
				if (fields.HasField(nameof(WorkflowProcess.Status))) m.Status = d.Status;
				if (fields.HasField(nameof(WorkflowProcess.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(WorkflowProcess.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!processStepFields.IsEmpty() && workflowProcessStepMap != null && workflowProcessStepMap.ContainsKey(d.Id)) m.Steps = workflowProcessStepMap[d.Id];
				if (!userFields.IsEmpty() && userMap != null && d.UserId.HasValue && userMap.ContainsKey(d.UserId.Value)) m.User = userMap[d.UserId.Value];
				if (!datasetFields.IsEmpty() && datasetMap != null && d.DatasetId.HasValue && datasetMap.ContainsKey(d.DatasetId.Value)) m.Dataset = datasetMap[d.DatasetId.Value];

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, List<WorkflowProcessStep>>> WorkflowProcessSteps(IFieldSet fields, IEnumerable<Data.WorkflowProcess> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.WorkflowProcessStep)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, List<WorkflowProcessStep>> itemMap = null;
			IFieldSet clone = new FieldSet(fields.Fields).Ensure(this.AsIndexer(nameof(WorkflowProcessStep.Process), nameof(WorkflowProcessStep.Id)));
			WorkflowProcessStepQuery query = this._queryFactory.Query<WorkflowProcessStepQuery>().DisableTracking().ProcessIds(datas.Select(x => x.Id).Distinct()).Authorize(this._authorize);
			itemMap = await this._builderFactory.Builder<WorkflowProcessStepBuilder>().Authorize(this._authorize).AsMasterKey(query, clone, x => x.Process.Id.Value);

			if (!fields.HasField(this.AsIndexer(nameof(WorkflowProcessStep.Process), nameof(WorkflowProcessStep.Id)))) itemMap.SelectMany(x => x.Value).Where(x => x != null && x.Process != null).ToList().ForEach(x => x.Process.Id = null);

			return itemMap;
		}

		private async Task<Dictionary<Guid, User>> CollectUsers(IFieldSet fields, IEnumerable<Data.WorkflowProcess> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.User)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, User> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(User.Id)))) itemMap = this.AsEmpty(datas.Where(x => x.UserId.HasValue).Select(x => x.UserId.Value).Distinct(), x => new User() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(User.Id));
				UserQuery query = this._queryFactory.Query<UserQuery>().DisableTracking().Ids(datas.Where(x => x.UserId.HasValue).Select(x => x.UserId.Value).Distinct()).Authorize(this._authorize);
				itemMap = await this._builderFactory.Builder<UserBuilder>().Authorize(this._authorize).AsForeignKey(query, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(User.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}

		private async Task<Dictionary<Guid, Dataset>> CollectDatasets(IFieldSet fields, IEnumerable<Data.WorkflowProcess> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.Dataset)).And("fields", fields).And("dataCount", datas?.Count()));
			Dictionary<Guid, Dataset> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(Dataset.Id)))) itemMap = this.AsEmpty(datas.Where(x => x.DatasetId.HasValue).Select(x => x.DatasetId.Value).Distinct(), x => new Dataset() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(Dataset.Id));
				List<Service.DataManagement.Model.Dataset> datasets = (await this._queryFactory.Query<DatasetHttpQuery>().Ids(datas.Where(x => x.DatasetId.HasValue).Select(x => x.DatasetId.Value).Distinct()).CollectAsync()).Items;
				itemMap = await this._builderFactory.Builder<DatasetBuilder>().Authorize(this._authorize).AsForeignKey(datasets, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(Dataset.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);
			return itemMap;
		}
	}
}
