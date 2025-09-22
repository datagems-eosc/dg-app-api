using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Query;
using Cite.Tools.Data.Query;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class WorkflowDefinitionBuilder : Builder<WorkflowDefinition, Service.Airflow.Model.AirflowDag>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public WorkflowDefinitionBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			ILogger<WorkflowDefinitionBuilder> logger): base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
		}

		public WorkflowDefinitionBuilder Authorize(AuthorizationFlags flags)
		{
			this._authorize = flags;
			return this;
		}

		public override async Task<List<WorkflowDefinition>> Build(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDag> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.Airflow.Model.AirflowDag)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty() || datas == null || !datas.Any()) return Enumerable.Empty<WorkflowDefinition>().ToList();

			IFieldSet workflowTaskFields = fields.ExtractPrefixed(this.AsPrefix(nameof(WorkflowDefinition.Tasks)));
			Dictionary<String, List<WorkflowTask>> workflowTaskMap = await this.CollectWorkflowTasks(workflowTaskFields, datas);

			List<WorkflowDefinition> results = new List<WorkflowDefinition>();

			foreach (Service.Airflow.Model.AirflowDag d in datas)
			{
				WorkflowDefinition m = new WorkflowDefinition();

				if (fields.HasField(nameof(WorkflowDefinition.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(WorkflowDefinition.Name))) m.Name = d.Name;
				if (fields.HasField(nameof(WorkflowDefinition.IsPaused))) m.IsPaused = d.IsPaused;
				if (fields.HasField(nameof(WorkflowDefinition.IsStale))) m.IsStale = d.IsStale;
				if (fields.HasField(nameof(WorkflowDefinition.LastParsedTime))) m.LastParsedTime = d.LastParsedTime;
				if (fields.HasField(nameof(WorkflowDefinition.LastExpired))) m.LastExpired = d.LastExpired;
				if (fields.HasField(nameof(WorkflowDefinition.BundleName))) m.BundleName = d.BundleName;
				if (fields.HasField(nameof(WorkflowDefinition.BundleVersion))) m.BundleVersion = d.BundleVersion;
				if (fields.HasField(nameof(WorkflowDefinition.RelativeFileLocation))) m.RelativeFileLocation = d.RelativeFileLocation;
				if (fields.HasField(nameof(WorkflowDefinition.FileLocation))) m.FileLocation = d.FileLocation;
				if (fields.HasField(nameof(WorkflowDefinition.FileToken))) m.FileToken = d.FileToken;
				if (fields.HasField(nameof(WorkflowDefinition.Description))) m.Description = d.Description;
				if (fields.HasField(nameof(WorkflowDefinition.TimetableSummary))) m.TimetableSummary = d.TimetableSummary;
				if (fields.HasField(nameof(WorkflowDefinition.TimetableDescription))) m.TimetableDescription = d.TimetableDescription;
				if (fields.HasField(nameof(WorkflowDefinition.Tags))) m.Tags = d.Tags?.Select(x => x.Name).ToList();
				if (fields.HasField(nameof(WorkflowDefinition.MaxActiveTasks))) m.MaxActiveTasks = d.MaxActiveTasks;
				if (fields.HasField(nameof(WorkflowDefinition.MaxActiveRuns))) m.MaxActiveRuns = d.MaxActiveRuns;
				if (fields.HasField(nameof(WorkflowDefinition.MaxConsecutiveFailedRuns))) m.MaxConsecutiveFailedRuns = d.MaxConsecutiveFailedRuns;
				if (fields.HasField(nameof(WorkflowDefinition.HasTaskConcurrencyLimits))) m.HasTaskConcurrencyLimits = d.HasTaskConcurrencyLimits;
				if (fields.HasField(nameof(WorkflowDefinition.HasImportErrors))) m.HasImportErrors = d.HasImportErrors;
				if (fields.HasField(nameof(WorkflowDefinition.NextLogicalDate))) m.NextLogicalDate = d.NextLogicalDate;
				if (fields.HasField(nameof(WorkflowDefinition.NextDataIntervalStart))) m.NextDataIntervalStart = d.NextDataIntervalStart;
				if (fields.HasField(nameof(WorkflowDefinition.NextDataIntervalEnd))) m.NextDataIntervalEnd = d.NextDataIntervalEnd;
				if (fields.HasField(nameof(WorkflowDefinition.NextRunAfter))) m.NextRunAfter = d.NextRunAfter;
				if (fields.HasField(nameof(WorkflowDefinition.Owners))) m.Owners = d.Owners;
				if (!workflowTaskFields.IsEmpty() && workflowTaskMap != null && workflowTaskMap.ContainsKey(d.Id)) m.Tasks = workflowTaskMap[d.Id];

				results.Add(m);
			}

			return results;
		}

		private async Task<Dictionary<String, List<WorkflowTask>>> CollectWorkflowTasks(IFieldSet fields, IEnumerable<Service.Airflow.Model.AirflowDag> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.WorkflowTask)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<String, List<WorkflowTask>> itemMap = new Dictionary<String, List<WorkflowTask>>();

			List<String> workflowIds = datas.Select(x => x.Id).Distinct().ToList();
			foreach(String workflowId in workflowIds)
			{
				List<Service.Airflow.Model.AirflowTask> taskDatas = await this._queryFactory.Query<WorkflowTaskHttpQuery>().WorkflowId(workflowId).CollectAsync();
				List<Model.WorkflowTask> taskModels = await this._builderFactory.Builder<WorkflowTaskBuilder>().Authorize(this._authorize).Build(fields, taskDatas);
				if(taskModels != null && taskModels.Count > 0) itemMap[workflowId] = taskModels;
			}

			return itemMap;
		}
	}
}
