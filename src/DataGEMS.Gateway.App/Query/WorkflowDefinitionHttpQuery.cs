using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Globalization;

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowDefinitionHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private String _id { get; set; }
		private List<WorkflowDefinitionKind> _kinds { get; set; }
		private String _like {  get; set; }
		private Boolean? _excludeStaled { get; set; }
		private Boolean? _includePaused { get; set; }
		private WorkflowRunState? _lastRunState { get; set; }
		private RangeOf<DateOnly?> _runStartRange { get; set; }
		private RangeOf<DateOnly?> _runEndRange { get; set; }
		private List<WorkflowRunState> _runState { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.Airflow.AirflowConfig _config;
		private readonly ILogger<WorkflowDefinitionHttpQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;

		public WorkflowDefinitionHttpQuery(
			IHttpClientFactory httpClientFactory,
			Service.Airflow.AirflowConfig config,
			ILogger<WorkflowDefinitionHttpQuery> logger,
			JsonHandlingService jsonHandlingService,
			LogCorrelationScope logCorrelationScope,
			Service.Airflow.IAirflowAccessTokenService airflowAccessTokenService,
			ErrorThesaurus errors)
		{
			this._httpClientFactory = httpClientFactory;
			this._config = config;
			this._logger = logger;
			this._airflowAccessTokenService = airflowAccessTokenService;
			this._jsonHandlingService = jsonHandlingService;
			this._logCorrelationScope = logCorrelationScope;
			this._errors = errors;
		}

		public WorkflowDefinitionHttpQuery Id(String id) { this._id = id; return this; }
		public WorkflowDefinitionHttpQuery Kinds(IEnumerable<WorkflowDefinitionKind> kinds) { this._kinds = kinds?.ToList(); return this; }
		public WorkflowDefinitionHttpQuery Kinds(WorkflowDefinitionKind kind) { this._kinds = kind.AsList(); return this; }
		public WorkflowDefinitionHttpQuery Like(String like) { this._like = like; return this; }
		public WorkflowDefinitionHttpQuery ExcludeStaled(Boolean? excludeStaled) { this._excludeStaled = excludeStaled; return this; }
		public WorkflowDefinitionHttpQuery IncludePaused(Boolean? includePaused) { this._includePaused = includePaused; return this; }
		public WorkflowDefinitionHttpQuery LastRunState(WorkflowRunState runState) { this._lastRunState = runState; return this; }
		public WorkflowDefinitionHttpQuery RunStates(IEnumerable<WorkflowRunState> runStates) { this._runState = runStates?.ToList(); return this; }
		public WorkflowDefinitionHttpQuery RunStates(WorkflowRunState runState) { this._runState = runState.AsList(); return this; }
		public WorkflowDefinitionHttpQuery RunStartRange(RangeOf<DateOnly?> runRange) { this._runStartRange = runRange; return this; }
		public WorkflowDefinitionHttpQuery RunEndRange(RangeOf<DateOnly?> runRange) { this._runEndRange = runRange; return this; }

		protected bool IsFalseQuery()
		{
			return this._kinds.IsNotNullButEmpty() || this._runState.IsNotNullButEmpty();
		}

		public async Task<Service.Airflow.Model.AirflowDag> ByIdAsync()
		{
			if (String.IsNullOrEmpty(this._id)) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DagByIdEndpoint.Replace("{id}", this._id)}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowDag model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDag>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}

		public async Task<List<Service.Airflow.Model.AirflowDag>> CollectAsync()
		{
			Service.Airflow.Model.AirflowDagList model = await this.CollectBaseAsync(false);
			return model?.Items ?? Enumerable.Empty<Service.Airflow.Model.AirflowDag>().ToList();
		}

		public async Task<int> CountAsync()
		{
			Service.Airflow.Model.AirflowDagList model = await this.CollectBaseAsync(true);
			return model?.TotalEntries ?? 0;
		}

		private async Task<Service.Airflow.Model.AirflowDagList> CollectBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DagListEndpoint}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			QueryString qs = new QueryString();
			if (this._kinds != null)
			{
				this._kinds.ForEach(x => qs = qs.Add("tags", x.ToString()));
				qs = qs.Add("tags_match_mode", "any");
			}
			if (!string.IsNullOrEmpty(this._like)) qs = qs.Add("dag_display_name_pattern", this._like);
			if (this._excludeStaled.HasValue) qs = qs.Add("exclude_stale", this._excludeStaled.Value ? "true" : "false");
			if (this._includePaused.HasValue) qs = qs.Add("paused", this._includePaused.Value ? "true" : "false");
			if (this._lastRunState.HasValue) qs = qs.Add("last_dag_run_state", this._lastRunState.ToString().ToLower());
			if (this._runStartRange != null)
			{
				if (this._runStartRange.Start.HasValue)
				{
					DateTime rangeStart = this._runStartRange.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					qs = qs.Add("dag_run_start_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
				}
				if (this._runStartRange.End.HasValue)
				{
					DateTime rangeEnd = this._runStartRange.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					qs = qs.Add("dag_run_start_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
				}
			}
			if (this._runEndRange != null)
			{
				if (this._runEndRange.Start.HasValue)
				{
					DateTime rangeStart = this._runEndRange.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					qs = qs.Add("dag_run_end_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
				}
				if (this._runEndRange.End.HasValue)
				{
					DateTime rangeEnd = this._runEndRange.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					qs = qs.Add("dag_run_end_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
				}
			}
			if (this._runState != null) this._kinds.ForEach(x => qs = qs.Add("dag_run_state", x.ToString()));

			String orderBy = this.ApplyOrdering();
			if (!String.IsNullOrEmpty(orderBy) && !useInCount) qs = qs.Add("order_by", orderBy);

			if (useInCount)
			{
				qs = qs.Add("offset", 0.ToString());
				qs = qs.Add("limit", 1.ToString());
			}
			else if (this.Page != null && !this.Page.IsEmpty)
			{
				if (this.Page.Offset >= 0) qs = qs.Add("offset", this.Page.Offset.ToString());
				if (this.Page.Size > 0) qs = qs.Add("limit", this.Page.Size.ToString());
			}

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowDagList model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDagList>(content);
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

			foreach(OrderingFieldResolver field in this.Order.Items.Select(x => new OrderingFieldResolver(x)).ToList())
			{
				if (field.Match(nameof(App.Model.WorkflowDefinition.Id))) return "dag_id";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.Name))) return "dag_display_name";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.IsPaused))) return "is_paused";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.IsStale))) return "is_stale";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.BundleName))) return "bundle_name";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.BundleVersion))) return "bundle_version";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.MaxActiveTasks))) return "max_active_tasks";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.MaxActiveRuns))) return "max_active_runs";
				else if (field.Match(nameof(App.Model.WorkflowDefinition.MaxConsecutiveFailedRuns))) return "max_consecutive_failed_dag_runs";
			}
			return null;
		}
	}
}
