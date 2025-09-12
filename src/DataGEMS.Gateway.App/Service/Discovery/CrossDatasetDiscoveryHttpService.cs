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
using DataGEMS.Gateway.App.Service.Discovery.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;

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
		private readonly QueryFactory _queryFactory;
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
			QueryFactory queryFactory,
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
			this._queryFactory = queryFactory;
			this._requestAccessToken = requestAccessToken;
			this._logger = logger;
			this._jsonHandlingService = jsonHandlingService;
			this._errors = errors;
			this._builderFactory = builderFactory;
		}

		public async Task<List<CrossDatasetDiscovery>> DiscoverAsync(DiscoverInfo request, IFieldSet fieldSet)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			DatasetSubsetInfo datasetSubset = await this.DiscoverDatasetSubset(request);
			if (datasetSubset.BreakQuery) return new List<CrossDatasetDiscovery>();

			CrossDatasetDiscoveryRequest httpRequestModel = new CrossDatasetDiscoveryRequest
			{
				Query = request.Query,
				ResultCount = request.ResultCount ?? this._config.DefaultResultCount,
				DatasetIds = datasetSubset.DatasetIds,
			};

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.SearchEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(httpRequestModel), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			String content = await this.SendRequest(httpRequest);
			Model.CrossDatasetDiscoveryResponse rawResponse = null;
			try { rawResponse = this._jsonHandlingService.FromJson<Model.CrossDatasetDiscoveryResponse>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.CrossDatasetDiscovery, this._logCorrelationScope.CorrelationId);
			}
			return await this._builderFactory.Builder<App.Model.Builder.CrossDatasetDiscoveryBuilder>().Authorize(AuthorizationFlags.Any).Build(fieldSet, rawResponse?.Results);
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.CrossDatasetDiscovery, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.CrossDatasetDiscovery, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			String content = await response.Content.ReadAsStringAsync();
			return content;
		}

		private class DatasetSubsetInfo
		{
			public Boolean BreakQuery { get; set; }
			public List<Guid> DatasetIds { get; set; }
		}

		private async Task<DatasetSubsetInfo> DiscoverDatasetSubset(DiscoverInfo request)
		{
			if ((request.DatasetIds == null || request.DatasetIds.Count == 0) &&
				(request.CollectionIds == null || request.CollectionIds.Count == 0) &&
				(request.UserCollectionIds == null || request.UserCollectionIds.Count == 0)) return new DatasetSubsetInfo() { BreakQuery = false, DatasetIds = null };

			List<List<Guid>> datasetIdsPerKind = new List<List<Guid>>();
			if (request.DatasetIds != null && request.DatasetIds.Count > 0) 
			{
				List<DataManagement.Model.Dataset> directDatasets = await this._queryFactory.Query<DatasetLocalQuery>()
					.Ids(request.DatasetIds)
					.DisableTracking()
					.Authorize(AuthorizationFlags.Any)
					.CollectAsyncAsModels();

				if (directDatasets.Count > 0) datasetIdsPerKind.Add(directDatasets.Select(x => x.Id).ToList());
			}
			if (request.CollectionIds != null && request.CollectionIds.Count > 0) 
			{
				List<DataManagement.Model.Dataset> collectionDatasets = await this._queryFactory.Query<DatasetLocalQuery>()
					.CollectionIds(request.CollectionIds)
					.DisableTracking()
					.Authorize(AuthorizationFlags.Any)
					.CollectAsyncAsModels();

				if (collectionDatasets.Count > 0) datasetIdsPerKind.Add(collectionDatasets.Select(x => x.Id).ToList());
			}
			if (request.UserCollectionIds != null && request.UserCollectionIds.Count > 0)
			{
				List<Guid> userCollectionDatasetIds = await this._queryFactory.Query<UserDatasetCollectionQuery>()
					.UserCollectionIds(request.UserCollectionIds)
					.DisableTracking()
					.Authorize(AuthorizationFlags.Any)
					.CollectAsync(x => x.DatasetId);

				List<DataManagement.Model.Dataset> userCollectionDatasets = await this._queryFactory.Query<DatasetLocalQuery>()
					.CollectionIds(userCollectionDatasetIds)
					.DisableTracking()
					.Authorize(AuthorizationFlags.Any)
					.CollectAsyncAsModels();

				if (userCollectionDatasetIds.Count > 0) datasetIdsPerKind.Add(userCollectionDatasets.Select(x => x.Id).ToList());
			}

			if (!datasetIdsPerKind.SelectMany(x => x).Any()) return new DatasetSubsetInfo() { BreakQuery = true };

			List<Guid> datasetIds = null;
			foreach (List<Guid> runner in datasetIdsPerKind)
			{
				if (datasetIds == null) { datasetIds = runner; continue; }
				List<Guid> union = datasetIds.Intersect(runner).ToList();
				//individual predicates acting as AND
				if(datasetIds.Count > 0 && union.Count == 0) return new DatasetSubsetInfo() { BreakQuery = true };
				datasetIds = union;
			}

			if (datasetIds == null || datasetIds.Count == 0) return new DatasetSubsetInfo() { BreakQuery = true };

			return new DatasetSubsetInfo() { BreakQuery = false, DatasetIds = datasetIds };
		}

	}
}
