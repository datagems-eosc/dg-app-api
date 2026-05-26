using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Discovery.Model;
using DataGEMS.Gateway.App.Service.InDatasetDiscovery.Model;
using DataGEMS.Gateway.App.Service.TaskOrchestrator;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;

namespace DataGEMS.Gateway.App.Service.InDatasetDiscovery
{
	public class InDatasetDiscoveryHttpService : IInDatasetDiscoveryService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly InDatasetDiscoveryHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<TaskOrchestratorService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly QueryFactory _queryFactory;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly Data.AppDbContext _dbContext;
		private readonly EventBroker _eventBroker;
		private readonly IAuthorizationService _authorizationService;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public InDatasetDiscoveryHttpService(
			IAccessTokenService accessTokenService,
			IHttpClientFactory httpClientFactory,
			InDatasetDiscoveryHttpConfig config,
			LogTrackingCorrelationConfig logTrackingCorrelationConfig,
			LogCorrelationScope logCorrelationScope,
			ILogger<TaskOrchestratorService> logger,
			RequestTokenIntercepted requestAccessToken,
			QueryFactory queryFactory,
			ErrorThesaurus errors,
			JsonHandlingService jsonHandlingService,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			Data.AppDbContext dbContext,
			EventBroker eventBroker,
			IAuthorizationService authorizationService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer)
		{
			this._accessTokenService = accessTokenService;
			this._httpClientFactory = httpClientFactory;
			this._config = config;
			this._logTrackingCorrelationConfig = logTrackingCorrelationConfig;
			this._logCorrelationScope = logCorrelationScope;
			this._logger = logger;
			this._requestAccessToken = requestAccessToken;
			this._queryFactory = queryFactory;
			this._errors = errors;
			this._jsonHandlingService = jsonHandlingService;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
			this._dbContext = dbContext;
			this._eventBroker = eventBroker;
			this._authorizationService = authorizationService;
			this._localizer = localizer;
		}

		public async Task<LanguagePilotResponse> LinguisticFeaturesAsync(LinguisticFeaturesRequest request)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.LinguisticFeaturesEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(request), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			String content = await this.SendRequest(httpRequest);
			LanguagePilotResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<LanguagePilotResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.InDatasetDiscovery, this._logCorrelationScope.CorrelationId);
			}
			return rawResponse;
		}


		private async Task<string> SendRequest(HttpRequestMessage request, TimeSpan? timeout = null)
		{
			HttpResponseMessage response = null;
			try
			{
				var chosenClient = this._httpClientFactory.CreateClient();
				if (timeout.HasValue) chosenClient.Timeout = timeout.Value;
				response = await chosenClient.SendAsync(request);
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.TaskOrchestrator, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.TaskOrchestrator, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			String content = await response.Content.ReadAsStringAsync();
			return content;
		}
	}
}
