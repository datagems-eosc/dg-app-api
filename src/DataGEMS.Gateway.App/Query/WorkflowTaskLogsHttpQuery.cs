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
	public class WorkflowTaskLogsHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private String _workflowTaskId { get; set; }
		private String _workflowId { get; set; }
		private String _workflowExecutionId { get; set; }
		private int? _tryNumber { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.Airflow.AirflowConfig _config;
		private readonly ILogger<WorkflowTaskLogsHttpQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;

		public WorkflowTaskLogsHttpQuery(
			IHttpClientFactory httpClientFactory,
			Service.Airflow.AirflowConfig config,
			ILogger<WorkflowTaskLogsHttpQuery> logger,
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
		public WorkflowTaskLogsHttpQuery WorkflowTaskId(string workflowTaskid) { this._workflowTaskId = workflowTaskid; return this; }
		public WorkflowTaskLogsHttpQuery WorkflowId(string workflowId) { this._workflowId = workflowId; return this; }
		public WorkflowTaskLogsHttpQuery WorkflowExecutionId(string workflowExecutionId) { this._workflowExecutionId = workflowExecutionId; return this; }
		public WorkflowTaskLogsHttpQuery TryNumber(int? tryNumber) { this._tryNumber = tryNumber; return this; }

		protected bool IsFalseQuery()
		{
			return !this._tryNumber.HasValue || String.IsNullOrEmpty(this._workflowTaskId) || String.IsNullOrEmpty(this._workflowId) || String.IsNullOrEmpty(this._workflowExecutionId);
		}

		public async Task<List<Service.Airflow.Model.AirflowTaskLog>> ByIdAsync()
		{
			if (!this._tryNumber.HasValue || 
				String.IsNullOrEmpty(this._workflowTaskId) || 
				this._workflowId == null || 
				String.IsNullOrEmpty(this._workflowExecutionId)) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.TaskInstanceLogsEndpoint.Replace("{tryNumber}", this._tryNumber.ToString()).Replace("{taskId}", this._workflowTaskId).Replace("{workflowId}", this._workflowId).Replace("{executionId}", this._workflowExecutionId)}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowTaskLogList model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowTaskLogList>(content);
				return model?.Content ?? Enumerable.Empty<Service.Airflow.Model.AirflowTaskLog>().ToList();
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

	}
}
