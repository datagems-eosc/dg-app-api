using Cite.Tools.Data.Builder;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace DataGEMS.Gateway.App.Service.DatasetRecommender
{
	public class DatasetRecommenderHttpService : IDatasetRecommenderService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly DatasetRecommenderHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<DatasetRecommenderHttpService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public DatasetRecommenderHttpService(
			IAccessTokenService accessTokenService,
			IHttpClientFactory httpClientFactory,
			DatasetRecommenderHttpConfig config,
			LogTrackingCorrelationConfig logTrackingCorrelationConfig,
			LogCorrelationScope logCorrelationScope,
			ILogger<DatasetRecommenderHttpService> logger,
			RequestTokenIntercepted requestAccessToken,
			ErrorThesaurus errors,
			JsonHandlingService jsonHandlingService,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver)
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

		public async Task<HashSet<Guid>> IsInRecommender(List<Guid> datasetIds)
		{
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.ExistEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(datasetIds), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			string content = await this.SendRequest(httpRequest);
			Dictionary<Guid, bool> rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<Dictionary<Guid, bool>>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DatasetRecommender, this._logCorrelationScope.CorrelationId);
			}
			HashSet<Guid> inRecommender = rawResponse?.Where(x => x.Value)?.Select(x => x.Key)?.ToHashSet() ?? new HashSet<Guid>();
			return inRecommender;
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DatasetRecommender, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				string errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				bool includeErrorPayload = response != null && (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent);
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DatasetRecommender, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			string content = await response.Content.ReadAsStringAsync();
			return content;
		}
	}
}
