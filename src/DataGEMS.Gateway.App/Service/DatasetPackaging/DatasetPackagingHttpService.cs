using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
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
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.DatasetPackaging.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace DataGEMS.Gateway.App.Service.DatasetPackaging
{
	public class DatasetPackagingHttpService: IDatasetPackagingService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly DatasetPackagingHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<DatasetPackagingHttpService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly QueryFactory _queryFactory;

		public DatasetPackagingHttpService(
			IAccessTokenService accessTokenService,
			IHttpClientFactory httpClientFactory,
			DatasetPackagingHttpConfig config,
			LogTrackingCorrelationConfig logTrackingCorrelationConfig,
			LogCorrelationScope logCorrelationScope,
			ILogger<DatasetPackagingHttpService> logger,
			RequestTokenIntercepted requestAccessToken,
			ErrorThesaurus errors,
			JsonHandlingService jsonHandlingService,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			QueryFactory queryFactory)
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
			this._queryFactory = queryFactory;
		}

		public async Task<HashSet<Guid>> IsInPackaging(List<Guid> datasetIds)
		{
			if (datasetIds == null || datasetIds.Count == 0) return new HashSet<Guid>();
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			string requestUrl = $"{this._config.BaseUrl}{this._config.ExistEndpoint}";
			string requestBody = this._jsonHandlingService.ToJson(new
			{
				ids = datasetIds
			});
			this._logger.Debug("Sending request to {url} with body {body}", requestUrl, requestBody);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
			{
				Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			string content = await this.SendRequest(httpRequest);
			MissingFromPackagingResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<MissingFromPackagingResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DatasetPackaging, this._logCorrelationScope.CorrelationId);
			}
			HashSet<Guid> inPackaging = datasetIds.Except(rawResponse.MissingIds).ToHashSet();
			return inPackaging;
		}

		public async Task<PackageRecommendation> RecommendAsync(PackageRecommendationRequest request, IFieldSet fields)
		{
			List<Guid> allowedDatasetIds = await this._authorizationContentResolver.EffectiveContextAffiliatedDatasets(Permission.CanRetrievePackage);
			if (request.DatasetIds != null && request.DatasetIds.Any(x => !allowedDatasetIds.Contains(x))) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.RecommendEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(new
				{
					ids = request.DatasetIds,
					k = request.DatasetsPerPackage,
					n = request.PackagesCount,
				}), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			string content = await this.SendRequest(httpRequest);
			DatasetPackagingRecommendationResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<DatasetPackagingRecommendationResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DatasetPackaging, this._logCorrelationScope.CorrelationId);
			}
			HashSet<Guid> datasetIds = rawResponse.Packages.SelectMany(x => x.DatasetIds).ToHashSet();
			List<Service.DataManagement.Model.Dataset> datasets = (await this._queryFactory.Query<DatasetHttpQuery>().Ids(datasetIds).CollectAsync())?.Items ?? [];
			List<App.Model.Dataset> models = await this._builderFactory.Builder<App.Model.Builder.DatasetBuilder>().Build(fields, datasets);
			return new PackageRecommendation
			{
				Packages = rawResponse.Packages.Select(x => new PackageRecommendation.Package
				{
					Name = x.Name,
					Datasets = x.DatasetIds.Where(id => models.Any(y => y.Id == id)).Select(id => models.First(y => y.Id == id)).ToList()
				}).ToList()
			};
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
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DatasetPackaging, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				string errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				bool includeErrorPayload = response != null && (response.StatusCode == System.Net.HttpStatusCode.BadRequest || response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent);
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DatasetPackaging, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			string content = await response.Content.ReadAsStringAsync();
			this._logger.Debug("Response content: {content}", content);
			return content;
		}
	}
}
