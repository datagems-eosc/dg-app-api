using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Service.DatasetLinking.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Service.DatasetLinking
{
	public class DatasetLinkingHttpService : IDatasetLinkingService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly DatasetLinkingHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<DatasetLinkingHttpService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public DatasetLinkingHttpService(
			IAccessTokenService accessTokenService,
			IHttpClientFactory httpClientFactory,
			DatasetLinkingHttpConfig config,
			LogTrackingCorrelationConfig logTrackingCorrelationConfig,
			LogCorrelationScope logCorrelationScope,
			ILogger<DatasetLinkingHttpService> logger,
			RequestTokenIntercepted requestAccessToken,
			ErrorThesaurus errors,
			JsonHandlingService jsonHandlingService,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver
		)
		{
			_accessTokenService = accessTokenService;
			_httpClientFactory = httpClientFactory;
			_config = config;
			_logTrackingCorrelationConfig = logTrackingCorrelationConfig;
			_logCorrelationScope = logCorrelationScope;
			_logger = logger;
			_requestAccessToken = requestAccessToken;
			_errors = errors;
			_jsonHandlingService = jsonHandlingService;
			_builderFactory = builderFactory;
			_authorizationContentResolver = authorizationContentResolver;
		}

		public async Task<DatasetLinkingStatus> GetJobStatusByIdAsync(Guid id)
		{
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			string requestUrl = $"{this._config.BaseUrl}{this._config.JobStatusEndpoint}".Replace("{jobId}", id.ToString());
			this._logger.Debug("Sending request to {requestUrl}", requestUrl);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			string content = await this.SendRequest(httpRequest);
			DatasetLinkingJobStatus rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<DatasetLinkingJobStatus>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DatasetLinking, this._logCorrelationScope.CorrelationId);
			}
			return rawResponse.Status;
		}

		public async Task<string> GetJobByIdAsync(Guid id)
		{
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			string requestUrl = $"{this._config.BaseUrl}{this._config.JobResultEndpoint}".Replace("{jobId}", id.ToString());
			this._logger.Debug("Sending request to {requestUrl}", requestUrl);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			return await this.SendRequest(httpRequest);
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try
			{
				response = await this._httpClientFactory.CreateClient().SendAsync(request);
				this._logger.Debug("Received response with status code {statusCode}", response?.StatusCode);
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.QueryRecommender, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				string errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				bool includeErrorPayload = response != null && (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent);
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.QueryRecommender, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			string content = await response.Content.ReadAsStringAsync();
			this._logger.Debug("Response content: {content}", content);
			return content;
		}
	}
}
