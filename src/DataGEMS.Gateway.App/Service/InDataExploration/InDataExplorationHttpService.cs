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
using DataGEMS.Gateway.App.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Service.InDataExploration
{
	public class InDataExplorationHttpService : IInDataExplorationService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly InDataExplorationHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<InDataExplorationHttpService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;

		public InDataExplorationHttpService(
			IHttpClientFactory httpClientFactory,
			IAccessTokenService accessTokenService,
			InDataExplorationHttpConfig config,
			LogTrackingCorrelationConfig logTrackingCorrelationConfig,
			LogCorrelationScope logCorrelationScope,
			RequestTokenIntercepted requestAccessToken,
			ILogger<InDataExplorationHttpService> logger,
			JsonHandlingService jsonHandlingService,
			ErrorThesaurus errors,
			BuilderFactory builderFactory)
		{
			this._httpClientFactory = httpClientFactory;
			this._accessTokenService = accessTokenService;
			this._config = config;
			this._logTrackingCorrelationConfig = logTrackingCorrelationConfig;
			this._logCorrelationScope = logCorrelationScope;
			this._requestAccessToken = requestAccessToken;
			this._logger = logger;
			this._jsonHandlingService = jsonHandlingService;
			this._errors = errors;
			this._builderFactory = builderFactory;
		}

		public async Task<InDataExplore> ExploreAsync(ExploreInfo request, IFieldSet fieldSet)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null)	throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.ExploreEndpoint}{new QueryString().Add("question", request.Question).ToString()}");
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			String content = await this.SendRequest(httpRequest);
			Model.InDataExplorationResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<Model.InDataExplorationResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.InDataExploration, this._logCorrelationScope.CorrelationId);
			}
			return await this._builderFactory.Builder<App.Model.Builder.InDataExplorationBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldSet, rawResponse);
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.InDataExploration, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.InDataExploration, this._logCorrelationScope.CorrelationId);
			}
			String content = await response.Content.ReadAsStringAsync();
			return content;
		}
	}
}
