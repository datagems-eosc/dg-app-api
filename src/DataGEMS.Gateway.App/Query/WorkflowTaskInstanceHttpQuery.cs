using System.Text;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowTaskInstanceHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private List<String> _taskIds { get; set; }
		private List<String> _workflowIds { get; set; }
		private List<String> _workflowExecutionIds { get; set; }
		private List<WorkflowTaskInstanceState> _state { get; set; }
		private RangeOf<DateOnly?> _runAfterRange { get; set; }
		private RangeOf<DateOnly?> _logicalDateRange { get; set; }
		private RangeOf<DateOnly?> _startDateRange { get; set; }
		private RangeOf<DateOnly?> _endDateRange { get; set; }
		private RangeOf<decimal?> _durationRange { get; set; }
		private List<String> _pool { get; set; }
		private List<String> _queue { get; set; }
		private List<String> _executor { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.Airflow.AirflowConfig _config;
		private readonly ILogger<WorkflowTaskInstanceHttpQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;

		public WorkflowTaskInstanceHttpQuery(
			IHttpClientFactory httpClientFactory,
			Service.Airflow.AirflowConfig config,
			ILogger<WorkflowTaskInstanceHttpQuery> logger,
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
		
		public WorkflowTaskInstanceHttpQuery TaskIds(IEnumerable<String> taskid) { this._taskIds = taskid.ToList(); return this; }
		public WorkflowTaskInstanceHttpQuery TaskIds(string taskid) { this._taskIds = taskid.AsList(); return this; }
		public WorkflowTaskInstanceHttpQuery WorkflowIds(IEnumerable<String> workflowId) { this._workflowIds = workflowId?.ToList(); return this; }
		public WorkflowTaskInstanceHttpQuery WorkflowIds(String workflowId) { this._workflowIds = workflowId.AsList(); return this; }
		public WorkflowTaskInstanceHttpQuery WorkflowExecutionIds(IEnumerable<String> workflowExecutionId) { this._workflowExecutionIds = workflowExecutionId?.ToList(); return this; }
		public WorkflowTaskInstanceHttpQuery WorkflowExecutionIds(String workflowExecutionId) { this._workflowIds = workflowExecutionId.AsList(); return this; }
		public WorkflowTaskInstanceHttpQuery State(IEnumerable<WorkflowTaskInstanceState> state) { this._state = state?.ToList(); return this; }
		public WorkflowTaskInstanceHttpQuery State(WorkflowTaskInstanceState state) { this._state = state.AsList(); return this; }
		public WorkflowTaskInstanceHttpQuery LogicalDateRange(RangeOf<DateOnly?> logicalDateRange) { this._logicalDateRange = logicalDateRange; return this; }
		public WorkflowTaskInstanceHttpQuery RunAfterRange(RangeOf<DateOnly?> runRange) { this._runAfterRange = runRange; return this; }
		public WorkflowTaskInstanceHttpQuery StartDateRange(RangeOf<DateOnly?> startRange) { this._startDateRange = startRange; return this; }
		public WorkflowTaskInstanceHttpQuery EndDateRange(RangeOf<DateOnly?> endRange) { this._endDateRange = endRange; return this; }
		public WorkflowTaskInstanceHttpQuery DurationRange(RangeOf<Decimal?> durationRange) { this._durationRange = durationRange; return this; }
		public WorkflowTaskInstanceHttpQuery Pool(IEnumerable<String> pool) { this._pool = pool.ToList(); return this; }
		public WorkflowTaskInstanceHttpQuery Pool(string pool) { this._pool = pool.AsList(); return this; }
		public WorkflowTaskInstanceHttpQuery Queue(IEnumerable<String> queue) { this._queue = queue.ToList(); return this; }
		public WorkflowTaskInstanceHttpQuery Queue(string queue) { this._queue = queue.AsList(); return this; }
		public WorkflowTaskInstanceHttpQuery Executor(IEnumerable<String> executor) { this._executor = executor.ToList(); return this; }
		public WorkflowTaskInstanceHttpQuery Executor(string executor) { this._executor = executor.AsList(); return this; }

		protected bool IsFalseQuery()
		{
			return this._pool.IsNotNullButEmpty() || this._queue.IsNotNullButEmpty() || this._executor.IsNotNullButEmpty() || this._state.IsNotNullButEmpty() || 
					this._taskIds.IsNotNullButEmpty() || this._workflowIds.IsNotNullButEmpty() || this._workflowExecutionIds.IsNotNullButEmpty();
		}

		public async Task<Service.Airflow.Model.AirflowTaskInstance> ByIdAsync()
		{
			if (this._workflowIds == null || this._workflowIds.Count != 1 || this._workflowExecutionIds == null || this._workflowExecutionIds.Count != 1 || this._taskIds == null || this._taskIds.Count != 1) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.TaskInstancesByIdEndpoint.Replace("{workflowId}", this._workflowIds[0]).Replace("{executionId}", this._workflowExecutionIds[0]).Replace("{id}", this._taskIds[0])}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowTaskInstance model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowTaskInstance>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}

		public async Task<List<Service.Airflow.Model.AirflowTaskInstance>> CollectAsync()
		{
			Service.Airflow.Model.AirflowTaskInstanceList model = await this.CollectBaseAsync(false);
			return model?.Items ?? Enumerable.Empty<Service.Airflow.Model.AirflowTaskInstance>().ToList();
		}

		public async Task<int> CountAsync()
		{
			Service.Airflow.Model.AirflowTaskInstanceList model = await this.CollectBaseAsync(true);
			return model?.TotalEntries ?? 0;
		}

		private async Task<Service.Airflow.Model.AirflowTaskInstanceList> CollectBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Service.Airflow.Model.AirflowTaskInstanceListRequest requestModel = new Service.Airflow.Model.AirflowTaskInstanceListRequest();
			if (this._taskIds != null && this._taskIds.Count > 0) requestModel.TaskIds = this._taskIds.Select(x => x.ToString()).ToList();
			if (this._workflowExecutionIds != null && this._workflowExecutionIds.Count > 0) requestModel.DagRunIds = this._workflowExecutionIds.Select(x => x.ToString()).ToList();
			if (this._workflowIds != null && this._workflowIds.Count > 0) requestModel.DagIds = this._workflowIds;
			if (this._state != null && this._state.Count > 0) requestModel.State = this._state.Select(x => x.ToString()).ToList();
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
				Service.Airflow.Model.AirflowTaskInstanceList model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowTaskInstanceList>(content);
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
			return content;
		}

		private String ApplyOrdering()
		{
			if (this.Order == null || this.Order.IsEmpty) return null;

			foreach (OrderingFieldResolver field in this.Order.Items.Select(x => new OrderingFieldResolver(x)).ToList())
			{
				if (field.Match(nameof(App.Model.WorkflowTaskInstance.Id))) return "id";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.WorkflowTaskId))) return "task_id";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.WorkflowExecutionId))) return "dag_run_id";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.WorkflowId))) return "dag_id";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.MapIndex))) return "map_index";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.LogicalDate))) return "logical_date";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.RunAfter))) return "run_after";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Start))) return "start_date";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.End))) return "end_date";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Duration))) return "duration";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.State))) return "state";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.TryNumber))) return "try_number";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.MaxTries))) return "max_tries";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.TaskDisplayName))) return "task_display_name";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Hostname))) return "hostname";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Unixname))) return "unixname";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Pool))) return "pool";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.PoolSlots))) return "pool_slots";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Queue))) return "queue";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.PriorityWeight))) return "priority_weight";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Operator))) return "operator";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.QueuedWhen))) return "queued_when";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.ScheduledWhen))) return "scheduled_when";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Pid))) return "pid";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Executor))) return "executor";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.ExecutorConfig))) return "executor_config";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.Note))) return "note";
				else if (field.Match(nameof(App.Model.WorkflowTaskInstance.RenderedMapIndex))) return "rendered_map_index";
			}
			return null;
		}
	}
}
