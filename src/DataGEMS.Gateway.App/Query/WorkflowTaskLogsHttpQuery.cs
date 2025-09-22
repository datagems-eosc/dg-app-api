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
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowTaskLogsHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private String _taskId { get; set; }
		private String _dagId { get; set; }
		private String _dagRunId { get; set; }
		private int _tryNumber { get; set; }
		private int _mapIndex { get; set; }
		private String _token { get; set; }

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
		public WorkflowTaskLogsHttpQuery TaskId(string taskid) { this._taskId= taskid; return this; }
		public WorkflowTaskLogsHttpQuery DagIds(string dagIds){this._dagId = dagIds;return this;}
		public WorkflowTaskLogsHttpQuery DagRunIds(string dagRunIds){this._dagRunId = dagRunIds;	return this;}
		public WorkflowTaskLogsHttpQuery TryNumber(int tryNumber){this._tryNumber = tryNumber;return this;}
		public WorkflowTaskLogsHttpQuery MapIndex(int mapIndex){this._mapIndex = mapIndex;return this;}
		public WorkflowTaskLogsHttpQuery Token(string token){this._token = token;return this;}

		protected bool IsFalseQuery()
		{
			return  this._taskId.IsNotNullButEmpty() || this._dagId.IsNotNullButEmpty() || this._dagRunId.IsNotNullButEmpty();
		}

		public async Task<Service.Airflow.Model.AirflowTaskLogs> ByIdAsync()
		{
			if (String.IsNullOrEmpty(this._taskId) || this._dagId == null || String.IsNullOrEmpty(this._dagRunId)) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.TaskInstanceLogsEndpoint}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowTaskLogs model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowTaskLogs>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}
		public async Task<List<Service.Airflow.Model.AirflowTaskLogs>> CollectAsync()
		{
			Service.Airflow.Model.AirflowTaskLogsList model = await this.CollectBaseAsync(false);
			return model?.Content ?? Enumerable.Empty<Service.Airflow.Model.AirflowTaskLogs>().ToList();
		} 
		public async Task<String> CountAsync()
		{
			Service.Airflow.Model.AirflowTaskLogsList model = await this.CollectBaseAsync(true);
			return model?.ContinuationToken;
		}
		private async Task<Service.Airflow.Model.AirflowTaskLogsList> CollectBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Service.Airflow.Model.AirflowTaskLogsRequest requestModel = new Service.Airflow.Model.AirflowTaskLogsRequest();
			
			if (!string.IsNullOrEmpty(this._taskId))requestModel.TaskId = this._taskId;
			if (!string.IsNullOrEmpty(this._dagId))requestModel.DagId = this._dagId;
			if (!string.IsNullOrEmpty(this._dagRunId))requestModel.DagRunId = this._dagRunId;
			if (this._tryNumber > 0)requestModel.TryNumber = this._tryNumber;
			if (this._mapIndex != -1)  requestModel.MapIndex = this._mapIndex;
			if (!string.IsNullOrEmpty(this._token))requestModel.Token = this._token;


			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.TaskInstanceLogsEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(requestModel), Encoding.UTF8, "application/json")
			};

			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowTaskLogsList model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowTaskLogsList>(content);
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
			return content; 
		}

	}
}
