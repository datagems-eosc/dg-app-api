using System.Text;
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
	public class WorkflowXcomEntryHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private String _taskId { get; set; }
		private String _workflowId { get; set; }
		private String _workflowExecutionId { get; set; }
		private String _xComKey { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.Airflow.AirflowConfig _config;
		private readonly ILogger<WorkflowXcomEntryHttpQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;

		public WorkflowXcomEntryHttpQuery(
			IHttpClientFactory httpClientFactory,
			Service.Airflow.AirflowConfig config,
			ILogger<WorkflowXcomEntryHttpQuery> logger,
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

		public WorkflowXcomEntryHttpQuery TaskIds(String taskid) { this._taskId = taskid; return this; }
		public WorkflowXcomEntryHttpQuery WorkflowIds(String workflowId) { this._workflowId = workflowId; return this; }
		public WorkflowXcomEntryHttpQuery WorkflowExecutionIds(String workflowExecutionId) { this._workflowExecutionId = workflowExecutionId; return this; }
		public WorkflowXcomEntryHttpQuery XComKey(String xComKey) { this._xComKey = xComKey; return this; }

		protected bool IsFalseQuery()
		{
			return String.IsNullOrEmpty(this._taskId) || String.IsNullOrEmpty(this._workflowId) || String.IsNullOrEmpty(this._workflowExecutionId);
		}

		public async Task<Service.Airflow.Model.AirflowXcomEntry> ByIdAsync()
		{
			if(String.IsNullOrEmpty(this._taskId) || String.IsNullOrEmpty(this._workflowId) || String.IsNullOrEmpty(this._workflowExecutionId) || String.IsNullOrEmpty(this._xComKey)) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.XComByIdEndpoint
				.Replace("{workflowId}", this._workflowId )
				.Replace("{executionId}", this._workflowExecutionId)
				.Replace("{taskId}", this._taskId)
				.Replace("{xcomKey}", this._xComKey)}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowXcomEntry model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowXcomEntry>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}

		public async Task<List<Service.Airflow.Model.AirflowXcomEntry>> CollectAsync()
		{
			Service.Airflow.Model.AirflowXcomEntryList model = await this.CollectBaseAsync(false);
			return model?.Items ?? Enumerable.Empty<Service.Airflow.Model.AirflowXcomEntry>().ToList();
		}

		public async Task<int> CountAsync()
		{
			Service.Airflow.Model.AirflowXcomEntryList model = await this.CollectBaseAsync(true);
			return model?.TotalEntries ?? 0;
		}

		private async Task<Service.Airflow.Model.AirflowXcomEntryList> CollectBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			QueryString qs = new QueryString();
			if (!String.IsNullOrEmpty(this._xComKey)) qs = qs.Add("xcom_key", this._xComKey);

			if (useInCount)
			{
				qs = qs.Add("offset", 0.ToString());
				qs = qs.Add("limit", 1.ToString());
			}
			else if (this.Page != null && !this.Page.IsEmpty)
			{
				if (this.Page.Offset >= 0) qs = qs.Add("offset", this.Page.Offset.ToString());
				if (this.Page.Size >= 0) qs = qs.Add("limit", this.Page.Size.ToString());
			}

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.XComListEndpoint
				.Replace("{workflowId}", this._workflowId)
				.Replace("{executionId}", this._workflowExecutionId)
				.Replace("{taskId}", this._taskId)}");

			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowXcomEntryList model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowXcomEntryList>(content);
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

	}
}

