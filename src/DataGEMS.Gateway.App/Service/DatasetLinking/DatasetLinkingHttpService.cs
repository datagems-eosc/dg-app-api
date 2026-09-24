using Cite.Tools.Data.Builder;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.DatasetLinking.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Cite.Tools.Cipher;
using System.Collections.Specialized;

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
		private readonly ICipherService _cipherService;
		private readonly CipherProfiles _cipherProfiles;

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
			IAuthorizationContentResolver authorizationContentResolver,
			ICipherService cipherService,
			CipherProfiles cipherProfiles
		)
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
			this._cipherService = cipherService;
			this._cipherProfiles = cipherProfiles;
		}


		public async Task<string> RefineLinkingAsync(DatasetLinkingRefinement model)
		{
			List<Guid> allowedDatasetIds = await this._authorizationContentResolver.EffectiveContextAffiliatedDatasets(Permission.LinkRefineDataset);
			if (!allowedDatasetIds.Contains(model.Id1.Value) || !allowedDatasetIds.Contains(model.Id2.Value)) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			string requestUrl = $"{this._config.BaseUrl}{this._config.StartRefineEndpoint}".Replace("{datasetId1}", model.Id1.Value.ToString()).Replace("{datasetId2}", model.Id2.Value.ToString());
			UriBuilder uriBuilder = new UriBuilder(requestUrl);
			NameValueCollection query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
			if (model.KeywordContribution.HasValue) query["kw"] = model.KeywordContribution.Value.ToString();
			if (model.HeadlineContribution.HasValue) query["head"] = model.HeadlineContribution.Value.ToString();
			if (model.DescriptionContribution.HasValue) query["desc"] = model.DescriptionContribution.Value.ToString();
			if (model.PairSimilarityThreshold.HasValue) query["th"] = model.PairSimilarityThreshold.Value.ToString();
			uriBuilder.Query = query.ToString();
			requestUrl = uriBuilder.Uri.ToString();

			this._logger.Debug("Sending request to {requestUrl}", requestUrl);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
			{
				Content = new FormUrlEncodedContent(new Dictionary<string, string>()),
			};
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);
			string content = await this.SendRequest(httpRequest);
			RefineLinkingResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<RefineLinkingResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DatasetLinking, this._logCorrelationScope.CorrelationId);
			}

			string encodedResponse = this._cipherService.EncryptSymetricAes($"{await this._authorizationContentResolver.CurrentUserId()}", this._cipherProfiles.GenericProfileName);
			encodedResponse = Uri.EscapeDataString(encodedResponse);
			return rawResponse.JobId + "_" + encodedResponse;
		}

		public async Task<DatasetLinkingStatus> GetJobStatusByIdAsync(string id)
		{
			string[] fragments = id.Split('_');
			if (fragments.Length != 2 || string.IsNullOrEmpty(fragments[0]) || string.IsNullOrEmpty(fragments[1])) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			try
			{
				string encodedUserId = Uri.UnescapeDataString(fragments[1]);
				string decodedUserId = this._cipherService.DecryptSymetricAes(encodedUserId, this._cipherProfiles.GenericProfileName);
				if (decodedUserId != (await this._authorizationContentResolver.CurrentUserId()).ToString()) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			}
			catch
			{
				throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			}

			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			string requestUrl = $"{this._config.BaseUrl}{this._config.JobStatusEndpoint}".Replace("{jobId}", fragments[0]);
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

		public async Task<string> GetJobByIdAsync(string id)
		{
			string[] fragments = id.Split('_');
			if (fragments.Length != 2 || string.IsNullOrEmpty(fragments[0]) || string.IsNullOrEmpty(fragments[1])) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			try
			{
				string encodedUserId = Uri.UnescapeDataString(fragments[1]);
				string decodedUserId = this._cipherService.DecryptSymetricAes(encodedUserId, this._cipherProfiles.GenericProfileName);
				if (decodedUserId != (await this._authorizationContentResolver.CurrentUserId()).ToString()) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			}
			catch
			{
				throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			}

			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			string requestUrl = $"{this._config.BaseUrl}{this._config.JobResultEndpoint}".Replace("{jobId}", fragments[0]);
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
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DatasetLinking, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				string errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				bool includeErrorPayload = response != null && (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent);
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DatasetLinking, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			string content = await response.Content.ReadAsStringAsync();
			this._logger.Debug("Response content: {content}", content);
			return content;
		}

	}
}
