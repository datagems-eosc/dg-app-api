using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Data;
using DataGEMS.Gateway.App.DataManagement;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.Discovery.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace DataGEMS.Gateway.App.Service.Discovery
{
    public class CrossDatasetDiscoveryHttpService : ICrossDatasetDiscoveryService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly CrossDatasetDiscoveryHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<CrossDatasetDiscoveryHttpService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;


		public CrossDatasetDiscoveryHttpService(
			IHttpClientFactory httpClientFactory,
			IAccessTokenService accessTokenService,
			CrossDatasetDiscoveryHttpConfig config,
			LogTrackingCorrelationConfig logTrackingCorrelationConfig,
			LogCorrelationScope logCorrelationScope,
			RequestTokenIntercepted requestAccessToken,
			ILogger<CrossDatasetDiscoveryHttpService> logger,
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

		public async Task<List<CrossDatasetDiscovery>> DiscoverAsync(DiscoverInfo request, IFieldSet fieldSet)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGUnderpinningException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			CrossDatasetDiscoveryRequest httpRequestModel = new CrossDatasetDiscoveryRequest
			{
				Query = request.Query,
				ResultCount = request.ResultCount ?? this._config.DefaultResultCount
			};

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(httpRequestModel), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);


			String content = await this.SendRequest(httpRequest);
			try
			{
                Model.CrossDatasetDiscoveryResponse rawResponse = this._jsonHandlingService.FromJson<Model.CrossDatasetDiscoveryResponse>(content);
				return await this._builderFactory.Builder<App.Model.Builder.CrossDatasetDiscoveryBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldSet, rawResponse?.Results);
			}
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = await this._httpClientFactory.CreateClient().SendAsync(request);
			try
			{
				response.EnsureSuccessStatusCode();
				String content = await response.Content.ReadAsStringAsync();
				return content;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete request. response was {response.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}

	}
}
