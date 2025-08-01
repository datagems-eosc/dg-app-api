using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Service.Airflow.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public class AirflowAccessTokenService : IAirflowAccessTokenService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IDistributedCache _cache;
		private readonly AirflowConfig _config;
		private readonly ILogger<AirflowAccessTokenService> _logger;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;

		public AirflowAccessTokenService(
			IHttpClientFactory httpClientFactory,
			IDistributedCache cache,
			AirflowConfig config,
			LogCorrelationScope logCorrelationScope,
			JsonHandlingService jsonHandlingService,
			ILogger<AirflowAccessTokenService> logger,
			ErrorThesaurus errors)
		{
			this._httpClientFactory = httpClientFactory;
			this._cache = cache;
			this._config = config;
			this._jsonHandlingService = jsonHandlingService;
			this._logger = logger;
			this._logCorrelationScope = logCorrelationScope;
			this._errors = errors;
		}

		public async Task<string> GetAirflowAccessTokenAsync()
		{
			AirflowAccessToken accessToken = await this.CacheLookupToken();
			if (accessToken != null) return accessToken.AccessToken;

			var tokenRequest = new
			{
				username = this._config.Username,
				password = this._config.Password,
			};

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.TokenEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(tokenRequest), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");

			HttpResponseMessage httpResponse = null;
			try { httpResponse = await this._httpClientFactory.CreateClient().SendAsync(httpRequest); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {httpResponse?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)httpResponse?.StatusCode, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}

			try { httpResponse.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await httpResponse.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", httpResponse?.StatusCode, errorPayload);
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)httpResponse?.StatusCode, UnderpinningServiceType.CrossDatasetDiscovery, this._logCorrelationScope.CorrelationId);
			}

			String content = await httpResponse.Content.ReadAsStringAsync();

			AirflowAccessToken token = null;
			if (content != null)
			{
				try { token = JsonConvert.DeserializeObject<AirflowAccessToken>(content); }
				catch (System.Exception ex)
				{
					this._logger.Error(ex, "could not retrieve access token");
					token = null;
				}
			}
			if (token == null) return null;

			await this.CacheUpdateToken(token);

			return token.AccessToken;

		}

		private async Task<AirflowAccessToken> CacheLookupToken()
		{
			String cacheKey = this.CacheKey();

			String content = await this._cache.GetStringAsync(cacheKey);

			AirflowAccessToken info = null;
			if (content != null)
			{
				try { info = JsonConvert.DeserializeObject<AirflowAccessToken>(content); }
				catch (System.Exception ex)
				{
					this._logger.Warning(ex, "could not deserialize access token from cache");
					info = null;
				}
			}

			return info;
		}

		private async Task CacheUpdateToken(AirflowAccessToken value)
		{
			String cacheKey = this.CacheKey();

			String payload = null;
			if (value != null)
			{
				try { payload = JsonConvert.SerializeObject(value); }
				catch (System.Exception ex)
				{
					this._logger.Warning(ex, "could not serialize access token for cache");
					payload = null;
				}
			}
			if (payload == null) return;

			//GOTCHA: The airflow access token does not expose the expiration. We need to parse the JWT and look in exp property. Fromn configuraiton, this seems to be 24 hours. So setting one hour for safety
			await this._cache.SetStringAsync(cacheKey, payload, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1)));
		}

		private String CacheKey()
		{
			return $"{nameof(DataGEMS.Gateway)}:Airflow:{nameof(AirflowAccessTokenService)}:{this._config.Username}:v0";
		}
	}
}
