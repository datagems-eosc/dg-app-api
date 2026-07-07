using Cite.Tools.Data.Builder;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Service.DatasetRecommender.Model;
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
		private readonly IAuthorizationService _authorizationService;

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
			IAuthorizationContentResolver authorizationContentResolver,
			IAuthorizationService authorizationService)
		{
			this._accessTokenService = accessTokenService;
			this._httpClientFactory = httpClientFactory;
			this._config = config;
			this._logTrackingCorrelationConfig = logTrackingCorrelationConfig;
			this._logCorrelationScope = logCorrelationScope;
			this._logger = logger;
			this._requestAccessToken = requestAccessToken;
			this._errors = errors;
			this._jsonHandlingService = jsonHandlingService;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
			this._authorizationService = authorizationService;
		}

		public async Task<HashSet<Guid>> IsInRecommender(List<Guid> datasetIds)
		{
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);
			string requestUrl = $"{this._config.BaseUrl}{this._config.ExistEndpoint}";
			string requestBody = this._jsonHandlingService.ToJson(datasetIds);
			this._logger.Debug("Sending request to {url} with body {body}", requestUrl, requestBody);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
			{
				Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
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

		public async Task<List<Guid>> RecommendAsync(Guid datasetId, uint? recommendationsCount)
		{
			List<Guid> datasetIds = await this._authorizationContentResolver.EffectiveContextAffiliatedDatasets(Permission.CanRecommend);
			if (datasetIds == null || !datasetIds.Contains(datasetId)) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.RecommendEndpoint}".Replace("{entityId}", datasetId.ToString()).Replace("{recommendationsCount}", recommendationsCount?.ToString() ?? this._config.DefaultRecommendationDatasets.ToString()));
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);
			string content = await this.SendRequest(httpRequest);
			DatasetRecommendationResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<DatasetRecommendationResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DatasetRecommender, this._logCorrelationScope.CorrelationId);
			}
			return rawResponse?.Recommendations?.Select(x => x.DatasetId)?.ToList() ?? [];
		}

		public async Task<MatheRecommendationResponse> RecommendMatheAsync(MatheRecommendationRequest request)
		{
			await this._authorizationService.AuthorizeForce(Permission.CanRecommendMathE);
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.MatheRecommendationsEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(request), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);
			string content = await this.SendRequest(httpRequest);
			MatheRecommendationResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<MatheRecommendationResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DatasetRecommender, this._logCorrelationScope.CorrelationId);
			}
			return rawResponse;
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { 
				response = await this._httpClientFactory.CreateClient().SendAsync(request);
				this._logger.Debug("Received response with status code {statusCode}", response?.StatusCode);
			}
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
			this._logger.Debug("Response content: {content}", content);
			return content;
		}
	}
}
