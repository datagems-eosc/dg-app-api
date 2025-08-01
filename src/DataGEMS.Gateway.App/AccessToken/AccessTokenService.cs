using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace DataGEMS.Gateway.App.AccessToken
{
	public class AccessTokenService : IAccessTokenService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IDistributedCache _cache;
		private readonly IdpClientConfig _config;
		private readonly ILogger<AccessTokenService> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;

		public AccessTokenService(
			IHttpClientFactory httpClientFactory,
			IDistributedCache cache,
			IdpClientConfig config,
			ILogger<AccessTokenService> logger,
			LogCorrelationScope logCorrelationScope,
			ErrorThesaurus errors)
		{
			this._httpClientFactory = httpClientFactory;
			this._cache = cache;
			this._config = config;
			this._logger = logger;
			this._errors = errors;
			this._logCorrelationScope = logCorrelationScope;
		}

		public async Task<string> GetClientAccessTokenAsync(String scope)
		{
			ClientAccessToken accessToken = await this.CacheLookupClient(scope);
			if (accessToken != null) return accessToken.AccessToken;

			Dictionary<string, string> contentPaylod = new Dictionary<string, string>
				{
					{ "grant_type", "client_credentials" },
					{ "client_id", this._config.ClientId },
					{ "client_secret", this._config.ClientSecret }
				};
			if (!String.IsNullOrEmpty(scope)) contentPaylod.Add("scope", scope);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._config.ClientAccessTokenUrl)
			{
				Content = new FormUrlEncodedContent(contentPaylod)
			};

			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DataManagement, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}

			String content = await response.Content.ReadAsStringAsync();

			ClientAccessToken token = null;
			if (content != null)
			{
				try { token = JsonConvert.DeserializeObject<ClientAccessToken>(content); }
				catch (System.Exception ex)
				{
					this._logger.Error(ex, "could not retrieve access token");
					token = null;
				}
			}
			if (token == null) return null;

			await this.CacheUpdateClient(scope, token);

			return token.AccessToken;
		}

		public async Task<string> GetExchangeAccessTokenAsync(String requestAccessToken, String scope)
		{
			if (String.IsNullOrEmpty(requestAccessToken))
			{
				this._logger.Error($"cannot exchange token without token for scope {scope}");
				return null;
			}

			ClientAccessToken accessToken = await this.CacheLookupExchange(requestAccessToken, scope);
			if (accessToken != null) return accessToken.AccessToken;

			Dictionary<string, string> contentPaylod = new Dictionary<string, string>
				{
					{ "grant_type", "urn:ietf:params:oauth:grant-type:token-exchange" },
					{ "subject_token", requestAccessToken },
					{ "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
					{ "requested_token_type", "urn:ietf:params:oauth:token-type:access_token" }
			};
			if (!String.IsNullOrEmpty(scope)) contentPaylod.Add("scope", scope);

			String clientbasicAuthentication = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this._config.ClientId}:{this._config.ClientSecret}"));

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._config.ClientAccessTokenUrl)
			{
				Content = new FormUrlEncodedContent(contentPaylod),
				Headers =
				{
					{ HeaderNames.Accept, "application/json"},
					{ HeaderNames.Authorization, $"Basic {clientbasicAuthentication}"}
				}
			};

			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}

			String content = await response.Content.ReadAsStringAsync();

			ClientAccessToken token = null;
			if (content != null)
			{
				try { token = JsonConvert.DeserializeObject<ClientAccessToken>(content); }
				catch (System.Exception ex)
				{
					this._logger.Error(ex, "could not retrieve access token");
					token = null;
				}
			}
			if (token == null) return null;

			await this.CacheUpdateExchange(requestAccessToken, scope, token);

			return token.AccessToken;
		}

		private async Task<ClientAccessToken> CacheLookupClient(String key)
		{
			String cacheKey = this.CacheKeyClient(key);
			return await this.CacheLookupBase(cacheKey);
		}

		private async Task<ClientAccessToken> CacheLookupExchange(String accessToken, String key)
		{
			String combinedKey = $"{accessToken.ToSha256()}_{key}";
			String cacheKey = this.CacheKeyExchange(combinedKey);
			return await this.CacheLookupBase(cacheKey);
		}

		private async Task<ClientAccessToken> CacheLookupBase(String cacheKey)
		{
			String content = await this._cache.GetStringAsync(cacheKey);

			ClientAccessToken info = null;
			if (content != null)
			{
				try { info = JsonConvert.DeserializeObject<ClientAccessToken>(content); }
				catch (System.Exception ex)
				{
					this._logger.Warning(ex, "could not deserialize access token from cache");
					info = null; 
				}
			}

			return info;
		}

		private async Task CacheUpdateClient(String key, ClientAccessToken value)
		{
			String cacheKey = this.CacheKeyClient(key);
			await this.CacheUpdateBase(cacheKey, value);
		}

		private async Task CacheUpdateExchange(String accessToken, String key, ClientAccessToken value)
		{
			String combinedKey = $"{accessToken.ToSha256()}_{key}";
			String cacheKey = this.CacheKeyExchange(combinedKey);
			await this.CacheUpdateBase(cacheKey, value);
		}

		private async Task CacheUpdateBase(String cacheKey, ClientAccessToken value)
		{
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
			if (payload == null || value.ExpiresIn <= 30) return;

			await this._cache.SetStringAsync(cacheKey, payload, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(value.ExpiresIn - 30)));
		}

		private String CacheKeyClient(String key)
		{
			return $"{nameof(DataGEMS.Gateway)}:{nameof(AccessTokenService)}:{this._config.ClientId}:no-exchange:{key ?? String.Empty}:v0";
		}
		private String CacheKeyExchange(String key)
		{
			return $"{nameof(DataGEMS.Gateway)}:{nameof(AccessTokenService)}:{this._config.ClientId}:exchange:{key ?? String.Empty}:v0";
		}
	}
}
