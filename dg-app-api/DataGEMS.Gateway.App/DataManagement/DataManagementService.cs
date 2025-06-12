using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.DataManagement.Model;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.App.DataManagement
{
    public class DataManagementService : IDataManagementService
    {
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly DataManagementConfig _config;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly ILogger<DataManagementService> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;

		public DataManagementService(
			IHttpClientFactory httpClientFactory,
			IAccessTokenService accessTokenService,
			DataManagementConfig config,
			RequestTokenIntercepted requestAccessToken,
			ILogger<DataManagementService> logger,
			JsonHandlingService jsonHandlingService,
			ErrorThesaurus errors)
		{
			this._httpClientFactory = httpClientFactory;
			this._accessTokenService = accessTokenService;
			this._config = config;
			this._requestAccessToken = requestAccessToken;
			this._logger = logger;
			this._jsonHandlingService = jsonHandlingService;
			this._errors = errors;
		}

		public async Task<List<Dataset>> Collect()
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGUnderpinningException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DatasetQueryEndpoint}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				List<DataManagement.Model.Dataset> models = this._jsonHandlingService.FromJson<List<DataManagement.Model.Dataset>>(content);
				return models;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}

		public async Task<int> Count()
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGUnderpinningException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DatasetQueryEndpoint}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				int count = Convert.ToInt32(content);
				return count;
			}
			catch(System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message);
			}
		}

		private async Task<String> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = await _httpClientFactory.CreateClient().SendAsync(request);
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
