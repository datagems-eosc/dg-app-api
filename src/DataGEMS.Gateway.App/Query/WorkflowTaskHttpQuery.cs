using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowTaskHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private List<String> _taskIds { get; set; }
		private List<String> _workflowIds { get; set; }
		private List<String> _workflowRunIds { get; set; }
		private RangeOf<DateOnly?> _logicalDateRange { get; set; }
		private RangeOf<DateOnly?> _startDateRange { get; set; }
		private RangeOf<DateOnly?> _endDateRange { get; set; }
		private RangeOf<DateOnly?> _runAfterRange { get; set; }
		private RangeOf<decimal?> _durationRange { get; set; }
		private List<WorkflowRunState> _state { get; set; }
		private List<String?> _pool { get; set; }
		private List<String?> _queue { get; set; }
		private List<String?> _executor { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.Airflow.AirflowConfig _config;
		private readonly ILogger<WorkflowTaskHttpQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;

		public WorkflowTaskHttpQuery(
			IHttpClientFactory httpClientFactory,
			Service.Airflow.AirflowConfig config,
			ILogger<WorkflowTaskHttpQuery> logger,
			JsonHandlingService jsonHandlingService,
			LogCorrelationScope logCorrelationScope,
			Service.Airflow.IAirflowAccessTokenService airflowAccessTokenService,
			ErrorThesaurus errors)
		{
			this._httpClientFactory = httpClientFactory;
			this._config = config;
			this._logger = logger;
			this._errors = errors;
			this._logCorrelationScope = logCorrelationScope;
			this._jsonHandlingService = jsonHandlingService;
			this._airflowAccessTokenService = airflowAccessTokenService;
		}
		
		public WorkflowTaskHttpQuery TaskIds(IEnumerable<String> taskid) { this._taskIds = taskid.ToList(); return this; }
		public WorkflowTaskHttpQuery TaskIds(string taskid) { this._taskIds = taskid.AsList(); return this; }
		public WorkflowTaskHttpQuery WorkflowIds(IEnumerable<String> workflowId) { this._workflowIds = workflowId?.ToList(); return this; }
		public WorkflowTaskHttpQuery WorkflowIds(String workflowId) { this._workflowIds = workflowId.AsList(); return this; }
		public WorkflowTaskHttpQuery WorkflowRunIds(IEnumerable<String> workflowRunId) { this._workflowRunIds = workflowRunId?.ToList(); return this; }
		public WorkflowTaskHttpQuery WorkflowRunIds(String workflowRunId) { this._workflowIds = workflowRunId.AsList(); return this; }
		public WorkflowTaskHttpQuery State(IEnumerable<WorkflowRunState> state) { this._state = state?.ToList(); return this; }
		public WorkflowTaskHttpQuery State(WorkflowRunState state) { this._state = state.AsList(); return this; }
		public WorkflowTaskHttpQuery LogicalDateRange(RangeOf<DateOnly?> logicalDateRange) { this._logicalDateRange = logicalDateRange; return this; }
		public WorkflowTaskHttpQuery RunAfterRange(RangeOf<DateOnly?> runRange) { this._runAfterRange = runRange; return this; }
		public WorkflowTaskHttpQuery StartDateRange(RangeOf<DateOnly?> startRange) { this._startDateRange = startRange; return this; }
		public WorkflowTaskHttpQuery EndDateRange(RangeOf<DateOnly?> endRange) { this._endDateRange = endRange; return this; }
		public WorkflowTaskHttpQuery DurationRange(RangeOf<Decimal?> durationRange) { this._durationRange = durationRange; return this; }
		public WorkflowTaskHttpQuery Pool(IEnumerable<String> pool) { this._pool = pool.ToList(); return this; }
		public WorkflowTaskHttpQuery Pool(string pool) { this._pool = pool.AsList(); return this; }
		public WorkflowTaskHttpQuery Queue(IEnumerable<String> queue) { this._queue = queue.ToList(); return this; }
		public WorkflowTaskHttpQuery Queue(string queue) { this._queue = queue.AsList(); return this; }
		public WorkflowTaskHttpQuery Executor(IEnumerable<String> executor) { this._executor = executor.ToList(); return this; }
		public WorkflowTaskHttpQuery Executor(string executor) { this._executor = executor.AsList(); return this; }


		protected bool IsFalseQuery()
		{
			return this._state.IsNotNullButEmpty() || this._taskIds.IsNotNullButEmpty() || this._workflowIds.IsNotNullButEmpty() || this._workflowRunIds.IsNotNullButEmpty();
		}


		public async Task<List<Service.Airflow.Model.AirflowTaskExecution>> CollectAsync()
		{
			Service.Airflow.Model.AirflowTaskList model = await this.CollectBaseAsync(false);
			return model?.Items ?? Enumerable.Empty<Service.Airflow.Model.AirflowTaskExecution>().ToList();
		} // also here it returns the models i want

		public async Task<int> CountAsync()
		{
			Service.Airflow.Model.AirflowTaskList model = await this.CollectBaseAsync(true);
			return model?.TotalEntries ?? 0;
		}

		private async Task<Service.Airflow.Model.AirflowTaskList> CollectBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Service.Airflow.Model.AirflowTaskListRequest requestModel = new Service.Airflow.Model.AirflowTaskListRequest();
			if (this._workflowIds != null && this._workflowIds.Count > 0) requestModel.DagIds = this._workflowIds;
			if (this._state != null && this._state.Count > 0) requestModel.State = this._state.Select(x => x.ToString()).ToList();
			if (this._workflowRunIds != null && this._workflowRunIds.Count > 0) requestModel.DagRunIds = this._workflowRunIds.Select(x => x.ToString()).ToList();
			if (this._taskIds != null && this._taskIds.Count > 0) requestModel.TaskIds = this._taskIds.Select(x => x.ToString()).ToList();
			if (this._queue != null && this._queue.Count > 0) requestModel.Queue = this._queue.Select(x => x.ToString()).ToList();
			if (this._executor != null && this._executor.Count > 0) requestModel.Executor = this._executor.Select(x => x.ToString()).ToList();
			if (this._pool != null && this._pool.Count > 0) requestModel.Pool = this._pool.Select(x => x.ToString()).ToList();

			if (this._runAfterRange != null)
			{
				if (this._runAfterRange.Start.HasValue)
				{
					DateTime rangeStart = this._runAfterRange.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					requestModel.RunAfterGte = rangeStart;
				}
				if (this._runAfterRange.End.HasValue)
				{
					DateTime rangeEnd = this._runAfterRange.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					requestModel.RunAfterLte = rangeEnd;
				}
			}
			if (this._durationRange != null)
			{
				if (this._durationRange.Start.HasValue)
				{
					requestModel.DurationGte = this._durationRange.Start.Value;

				}
				if (this._durationRange.End.HasValue)
				{
					requestModel.DurationLte = this._durationRange.End.Value;
				}
			}
			if (this._logicalDateRange != null)
			{
				if (this._logicalDateRange.Start.HasValue)
				{
					DateTime rangeStart = this._logicalDateRange.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					requestModel.LogicalDateGte = rangeStart;
				}
				if (this._logicalDateRange.End.HasValue)
				{
					DateTime rangeEnd = this._logicalDateRange.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					requestModel.LogicalDateLte = rangeEnd;
				}
			}
			if (this._startDateRange != null)
			{
				if (this._startDateRange.Start.HasValue)
				{
					DateTime rangeStart = this._startDateRange.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					requestModel.StartDateGte = rangeStart;
				}
				if (this._startDateRange.End.HasValue)
				{
					DateTime rangeEnd = this._startDateRange.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					requestModel.StartDateLte = rangeEnd;
				}
			}
			if (this._endDateRange != null)
			{
				if (this._endDateRange.Start.HasValue)
				{
					DateTime rangeStart = this._endDateRange.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					requestModel.EndDateGte = rangeStart;
				}
				if (this._endDateRange.End.HasValue)
				{
					DateTime rangeEnd = this._endDateRange.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					requestModel.EndDateLte = rangeEnd;
				}
			}

			String orderBy = this.ApplyOrdering();
			if (!String.IsNullOrEmpty(orderBy) && !useInCount) requestModel.OrderBy = orderBy;

			if (useInCount)
			{
				requestModel.Offset = 0;
				requestModel.Limit = 1;
			}
			else if (this.Page != null && !this.Page.IsEmpty)
			{
				if (this.Page.Offset >= 0) requestModel.Offset = this.Page.Offset;
				if (this.Page.Size > 0) requestModel.Limit = this.Page.Size;
			}

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.TaskInstancesListEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(requestModel), Encoding.UTF8, "application/json")
			};

			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowTaskList model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowTaskList>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}


		private async Task<String> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			String content = await response.Content.ReadAsStringAsync();
			Console.WriteLine(content);
			return content; // here it gets the whole content
		}

		private String ApplyOrdering()
		{
			if (this.Order == null || this.Order.IsEmpty) return null;

			foreach (OrderingFieldResolver field in this.Order.Items.Select(x => new OrderingFieldResolver(x)).ToList())
			{
				if (field.Match(nameof(App.Model.WorkflowTasks.Id))) return "id";
				else if (field.Match(nameof(App.Model.WorkflowTasks.TaskId))) return "task_id";
				else if (field.Match(nameof(App.Model.WorkflowTasks.DagRunId))) return "dag_run_id";
				else if (field.Match(nameof(App.Model.WorkflowTasks.DagId))) return "dag_id";
				else if (field.Match(nameof(App.Model.WorkflowTasks.MapIndex))) return "map_index";
				else if (field.Match(nameof(App.Model.WorkflowTasks.LogicalDate))) return "logical_date";
				else if (field.Match(nameof(App.Model.WorkflowTasks.RunAfter))) return "run_after";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Start))) return "start_date";
				else if (field.Match(nameof(App.Model.WorkflowTasks.End))) return "end_date";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Duration))) return "duration";
				else if (field.Match(nameof(App.Model.WorkflowTasks.State))) return "state";
				else if (field.Match(nameof(App.Model.WorkflowTasks.TryNumber))) return "try_number";
				else if (field.Match(nameof(App.Model.WorkflowTasks.MaxTries))) return "max_tries";
				else if (field.Match(nameof(App.Model.WorkflowTasks.TaskDisplayName))) return "task_display_name";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Hostname))) return "hostname";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Unixname))) return "unixname";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Pool))) return "pool";
				else if (field.Match(nameof(App.Model.WorkflowTasks.PoolSlots))) return "pool_slots";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Queue))) return "queue";
				else if (field.Match(nameof(App.Model.WorkflowTasks.PriorityWeight))) return "priority_weight";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Operator))) return "operator";
				else if (field.Match(nameof(App.Model.WorkflowTasks.QueuedWhen))) return "queued_when";
				else if (field.Match(nameof(App.Model.WorkflowTasks.ScheduledWhen))) return "scheduled_when";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Pid))) return "pid";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Executor))) return "executor";
				else if (field.Match(nameof(App.Model.WorkflowTasks.ExecutorConfig))) return "executor_config";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Note))) return "note";
				else if (field.Match(nameof(App.Model.WorkflowTasks.RenderedMapIndex))) return "rendered_map_index";
				else if (field.Match(nameof(App.Model.WorkflowTasks.RenderedFields))) return "rendered_fields";
				else if (field.Match(nameof(App.Model.WorkflowTasks.Trigger))) return "trigger";
				else if (field.Match(nameof(App.Model.WorkflowTasks.TriggererJob))) return "triggerer_job";
				else if (field.Match(nameof(App.Model.WorkflowTasks.DagVersion))) return "dag_version";

			}
			return null;
		}
	}
}
