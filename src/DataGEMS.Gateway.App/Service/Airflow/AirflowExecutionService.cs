using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Airflow;
using DataGEMS.Gateway.App.Service.Airflow.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public class AirflowExecutionService : IAirflowExecutionService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<AirflowExecutionService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly QueryFactory _queryFactory;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;
		private readonly AirflowConfig _airflowConfig;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;


		public AirflowExecutionService(
		IHttpClientFactory httpClientFactory,
		IAccessTokenService accessTokenService,
		LogTrackingCorrelationConfig logTrackingCorrelationConfig,
		LogCorrelationScope logCorrelationScope,
		RequestTokenIntercepted requestAccessToken,
		QueryFactory queryFactory,
		ILogger<AirflowExecutionService> logger,
		JsonHandlingService jsonHandlingService,
		ErrorThesaurus errors,
		BuilderFactory builderFactory,
		AirflowConfig airflowConfig,
		Service.Airflow.IAirflowAccessTokenService airflowAccessTokenService)
		{
			this._httpClientFactory = httpClientFactory;
			this._accessTokenService = accessTokenService;
			this._logTrackingCorrelationConfig = logTrackingCorrelationConfig;
			this._logCorrelationScope = logCorrelationScope;
			this._queryFactory = queryFactory;
			this._requestAccessToken = requestAccessToken;
			this._logger = logger;
			this._jsonHandlingService = jsonHandlingService;
			this._errors = errors;
			this._builderFactory = builderFactory;
			this._airflowConfig = airflowConfig;
			this._airflowAccessTokenService = airflowAccessTokenService;
		}

		public async Task<WorkflowExecution> ExecutionDagAsync(string dagId, IFieldSet fieldset)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Service.Airflow.Model.AirflowExecutionRequest httpRequestModel = new Service.Airflow.Model.AirflowExecutionRequest
			{
				LogicalDate = DateTime.UtcNow
			};

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._airflowConfig.BaseUrl}{this._airflowConfig.DagRunEndpoint.Replace("{id}", dagId)}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(httpRequestModel), Encoding.UTF8, "application/json")
			};

			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String content = await this.SendRequest(httpRequest);
			Model.AirflowDagExecution rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<Airflow.Model.AirflowDagExecution>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
			
			WorkflowExecution result = await this._builderFactory.Builder<App.Model.Builder.WorkflowExecutionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldset, rawResponse);

			return  result ;
		}

		public async Task<WorkflowListExecution> ListofExecutionAsync(string dagId , IFieldSet fieldset)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Service.Airflow.Model.AirflowListExecutionRequest httpRequestModel = new Service.Airflow.Model.AirflowListExecutionRequest
			{
				DagId = dagId
			};

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._airflowConfig.BaseUrl}{this._airflowConfig.DagExecutionListEndpoint.Replace("{id}", dagId)}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(httpRequestModel), Encoding.UTF8, "application/json")
			};

			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String content = await this.SendRequest(httpRequest);
			Model.AirflowDagListExecution rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<Airflow.Model.AirflowDagListExecution>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}

			WorkflowListExecution result = await this._builderFactory.Builder<App.Model.Builder.WorkflowListExecutionBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldset, rawResponse);

			return result;
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request,the response was {response?.StatusCode}");
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
