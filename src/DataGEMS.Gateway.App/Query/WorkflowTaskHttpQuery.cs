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

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowTaskHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private String _workflowId { get; set; }
		private String _taskId { get; set; }

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
		
		public WorkflowTaskHttpQuery WorkflowId(String workflowId) { this._workflowId = workflowId; return this; }
		public WorkflowTaskHttpQuery TaskId(String taskId) { this._taskId = taskId; return this; }

		protected bool IsFalseQuery()
		{
			return String.IsNullOrEmpty(this._workflowId);
		}

		public async Task<Service.Airflow.Model.AirflowTask> ByIdAsync()
		{
			if (String.IsNullOrEmpty(this._workflowId) || String.IsNullOrEmpty(this._taskId)) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.TaskByIdEndpoint.Replace("{workflowId}", this._workflowId).Replace("{taskId}", this._taskId)}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowTask model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowTask>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}

		public async Task<List<Service.Airflow.Model.AirflowTask>> CollectAsync()
		{
			Service.Airflow.Model.AirflowTaskList model = await this.CollectBaseAsync(false);
			return model?.Items ?? Enumerable.Empty<Service.Airflow.Model.AirflowTask>().ToList();
		}

		public async Task<int> CountAsync()
		{
			Service.Airflow.Model.AirflowTaskList model = await this.CollectBaseAsync(true);
			return model?.TotalEntries ?? 0;
		}

		private async Task<Service.Airflow.Model.AirflowTaskList> CollectBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			QueryString qs = new QueryString();
			String orderBy = this.ApplyOrdering();
			if (!String.IsNullOrEmpty(orderBy) && !useInCount) qs = qs.Add("order_by", orderBy);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.TaskListEndpoint.Replace("{workflowId}", this._workflowId)}{qs.ToString()}");
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
			return content;
		}

		private String ApplyOrdering()
		{
			if (this.Order == null || this.Order.IsEmpty) return null;

			foreach (OrderingFieldResolver field in this.Order.Items.Select(x => new OrderingFieldResolver(x)).ToList())
			{
				if (field.Match(nameof(App.Model.WorkflowTask.Id))) return "task_id";
				else if (field.Match(nameof(App.Model.WorkflowTask.TaskDisplayName))) return "task_display_name";
				else if (field.Match(nameof(App.Model.WorkflowTask.Owner))) return "owner";
				else if (field.Match(nameof(App.Model.WorkflowTask.Start))) return "start_date";
				else if (field.Match(nameof(App.Model.WorkflowTask.End))) return "end_date";
				else if (field.Match(nameof(App.Model.WorkflowTask.TriggerRule))) return "trigger_rule";
				else if (field.Match(nameof(App.Model.WorkflowTask.DependsOnPast))) return "depends_on_past";
				else if (field.Match(nameof(App.Model.WorkflowTask.WaitForDownstream))) return "wait_for_downstream";
				else if (field.Match(nameof(App.Model.WorkflowTask.Retries))) return "retries";
				else if (field.Match(nameof(App.Model.WorkflowTask.Queue))) return "queue";
				else if (field.Match(nameof(App.Model.WorkflowTask.Pool))) return "pool";
				else if (field.Match(nameof(App.Model.WorkflowTask.PoolSlots))) return "pool_slots";
				else if (field.Match(nameof(App.Model.WorkflowTask.RetryExponentialBackoff))) return "retry_exponential_backoff";
				else if (field.Match(nameof(App.Model.WorkflowTask.PriorityWeight))) return "priority_weight";
				else if (field.Match(nameof(App.Model.WorkflowTask.WeightRule))) return "weight_rule";
				else if (field.Match(nameof(App.Model.WorkflowTask.OperatorName))) return "operator_name";
				else if (field.Match(nameof(App.Model.WorkflowTask.IsMapped))) return "is_mapped";
			}
			return null;
		}
	}
}
