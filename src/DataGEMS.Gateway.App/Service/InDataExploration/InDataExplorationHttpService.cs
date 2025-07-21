using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.InDataExploration.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;

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


		public async Task<List<InDataGeoQueryExploration>> ExploreGeoQueryAsync(ExploreGeoQueryInfo request, IFieldSet fieldSet)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGUnderpinningException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);


			string encodedQuestion = Uri.EscapeDataString(request.Question);
			string url = $"{this._config.BaseUrl}{this._config.GeoQueryEndpoint}?question={encodedQuestion}";

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);


			String content = await this.SendRequest(httpRequest);
			try
			{
				Model.ExplorationGeoQueryResponse rawResponse = this._jsonHandlingService.FromJson<Model.ExplorationGeoQueryResponse>(content);
				return await this._builderFactory.Builder<App.Model.Builder.InDataExplorationGeoQueryBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldSet, new List<Model.ExplorationGeoQueryResponse> { rawResponse });
			}
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}

		public async Task<List<InDataTextToSqlExploration>> ExploreTextToSqlAsync(ExploreTextToSqlInfo request, IFieldSet fieldSet)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGUnderpinningException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			ExplorationTextToSqlRequest httpRequestModel = new ExplorationTextToSqlRequest
			{
				Question = request.Question,
				Parameters = request.Parameters
			};

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.TextToSqlEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(httpRequestModel), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			String content = await this.SendRequest(httpRequest);
			try
			{
				Model.ExplorationTextToSqlResponse rawResponse = this._jsonHandlingService.FromJson<Model.ExplorationTextToSqlResponse>(content);
				return await this._builderFactory.Builder<App.Model.Builder.InDataExplorationTextToSqlBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldSet, new List<Model.ExplorationTextToSqlResponse> { rawResponse });
			}
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}

		public async Task<List<InDataSimpleExploreExploration>> ExploreSimpleExploreAsync(ExploreSimpleExploreInfo request, IFieldSet fieldSet)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null)	throw new DGUnderpinningException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			// Prepare URL
			string encodedQuestion = Uri.EscapeDataString(request.Question);
			string url = $"{this._config.BaseUrl}{this._config.SimpleExploreEndpoint}?question={encodedQuestion}";

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			string content = await this.SendRequest(httpRequest);

			try
			{
				Model.ExplorationSimpleExploreResponse rawResponse = this._jsonHandlingService.FromJson<Model.ExplorationSimpleExploreResponse>(content);
				return await this._builderFactory.Builder<App.Model.Builder.InDataExplorationSimpleExploreBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldSet, new List<Model.ExplorationSimpleExploreResponse> { rawResponse });
			}
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try
			{
				response = await this._httpClientFactory.CreateClient().SendAsync(request);
				response.EnsureSuccessStatusCode();
				String content = await response.Content.ReadAsStringAsync();
				return content;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}
	}
}
