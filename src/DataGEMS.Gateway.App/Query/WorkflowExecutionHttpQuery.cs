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
	public class WorkflowExecutionHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private String _id { get; set; }
		private List<String> _workflowIds { get; set; }
		private RangeOf<DateOnly?> _logicalDateRange { get; set; }
		private RangeOf<DateOnly?> _startDateRange { get; set; }
		private RangeOf<DateOnly?> _endDateRange { get; set; }
		private RangeOf<DateOnly?> _runAfterRange {  get; set; }
		private List<WorkflowRunType> _runType { get; set; }
		private List<WorkflowRunState> _state { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.Airflow.AirflowConfig _config;
		private readonly ILogger<WorkflowExecutionHttpQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;

		public WorkflowExecutionHttpQuery(
			IHttpClientFactory httpClientFactory,
			Service.Airflow.AirflowConfig config,
			ILogger<WorkflowExecutionHttpQuery> logger,
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

		public WorkflowExecutionHttpQuery Id(string id) { this._id = id; return this; }
		public WorkflowExecutionHttpQuery WorkflowIds(IEnumerable<String> workflowId) { this._workflowIds = workflowId?.ToList(); return this; }
		public WorkflowExecutionHttpQuery WorkflowIds(String workflowId) { this._workflowIds = workflowId.AsList(); return this; }
		public WorkflowExecutionHttpQuery State(IEnumerable<WorkflowRunState> state) { this._state = state?.ToList(); return this; }
		public WorkflowExecutionHttpQuery State(WorkflowRunState state){ this._state = state.AsList(); return this; }
		public WorkflowExecutionHttpQuery RunType(WorkflowRunType runType) { this._runType = runType.AsList(); return this; }
		public WorkflowExecutionHttpQuery RunType(IEnumerable<WorkflowRunType> runType) { this._runType = runType?.ToList(); return this; }
		public WorkflowExecutionHttpQuery LogicalDateRange(RangeOf<DateOnly?> logicalDateRange) { this._logicalDateRange = logicalDateRange; return this; }
		public WorkflowExecutionHttpQuery RunAfterRange(RangeOf<DateOnly?> runRange) {  this._runAfterRange = runRange; return this; }
		public WorkflowExecutionHttpQuery StartDateRange(RangeOf<DateOnly?> startRange) { this._startDateRange = startRange; return this; }
		public WorkflowExecutionHttpQuery EndDateRange(RangeOf<DateOnly?> endRange) { this._endDateRange = endRange; return this; }

		protected bool IsFalseQuery()
		{
			return this._state.IsNotNullButEmpty() || this._runType.IsNotNullButEmpty() || this._workflowIds.IsNotNullButEmpty();
		}

		public async Task<Service.Airflow.Model.AirflowDagExecution> ByIdAsync()
		{
			if (String.IsNullOrEmpty(this._id) || this._workflowIds == null || this._workflowIds.Count != 1) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DagExecutionByIdEndpoint.Replace("{workflowId}", this._workflowIds[0]).Replace("{id}", this._id)}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowDagExecution model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDagExecution>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}

		public async Task<List<Service.Airflow.Model.AirflowDagExecution>> CollectAsync()
		{
			Service.Airflow.Model.AirflowDagExecutionList model = await this.CollectBaseAsync(false);
			return model?.Items ?? Enumerable.Empty<Service.Airflow.Model.AirflowDagExecution>().ToList();
		}

		public async Task<int> CountAsync()
		{
			Service.Airflow.Model.AirflowDagExecutionList model = await this.CollectBaseAsync(true);
			return model?.TotalEntries ?? 0;
		}

		private async Task<Service.Airflow.Model.AirflowDagExecutionList> CollectBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Service.Airflow.Model.AirflowDagExecutionListRequest requestModel = new Service.Airflow.Model.AirflowDagExecutionListRequest();
			if (this._workflowIds != null && this._workflowIds.Count > 0) requestModel.DagIds = this._workflowIds;
			if (this._state != null && this._state.Count > 0) requestModel.States = this._state.Select(x => x.ToString()).ToList();
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

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DagExecutionListEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(requestModel), Encoding.UTF8, "application/json")
			};

			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowDagExecutionList model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDagExecutionList>(content);
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
				if (field.Match(nameof(App.Model.WorkflowExecution.Id))) return "dag_run_id";
				else if (field.Match(nameof(App.Model.WorkflowExecution.WorkflowId))) return "dag_id";
				else if (field.Match(nameof(App.Model.WorkflowExecution.LogicalDate))) return "logical_date";
				else if (field.Match(nameof(App.Model.WorkflowExecution.QueuedAt))) return "queued_At";
				else if (field.Match(nameof(App.Model.WorkflowExecution.Start))) return "start_date";
				else if (field.Match(nameof(App.Model.WorkflowExecution.End))) return "end_date";
				else if (field.Match(nameof(App.Model.WorkflowExecution.DataIntervalStart))) return "data_interval_start";
				else if (field.Match(nameof(App.Model.WorkflowExecution.DataIntervalEnd))) return "data_interval_end";
				else if (field.Match(nameof(App.Model.WorkflowExecution.RunAfter))) return "run_after";
				else if (field.Match(nameof(App.Model.WorkflowExecution.LastSchedulingDecision))) return "last_scheduling_decision";
				else if (field.Match(nameof(App.Model.WorkflowExecution.RunType))) return "run_type";
				else if (field.Match(nameof(App.Model.WorkflowExecution.TriggeredBy))) return "triggered_by";
				else if (field.Match(nameof(App.Model.WorkflowExecution.State))) return "state";
				else if (field.Match(nameof(App.Model.WorkflowExecution.BundleVersion))) return "bundle_version";
			}
			return null;
		}
	}
}
