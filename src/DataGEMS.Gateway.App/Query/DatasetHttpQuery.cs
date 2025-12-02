using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Service.DataManagement;
using DataGEMS.Gateway.App.Service.DataManagement.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Query
{
	public class DatasetHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _collectionIds { get; set; }
		private List<string> _properties { get; set; }
		private List<string> _types { get; set; }
		private Common.Enum.DatasetState? _datasetStatus { get; set; }
		private DateTime? _publishedDateFrom { get; set; }
		private DateTime? _publishedDateTo { get; set; }

		private String _like { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly DataManagementHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<DatasetHttpQuery> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;

		public DatasetHttpQuery(
			IHttpClientFactory httpClientFactory,
			IAccessTokenService accessTokenService,
			DataManagementHttpConfig config,
			LogTrackingCorrelationConfig logTrackingCorrelationConfig,
			LogCorrelationScope logCorrelationScope,
			RequestTokenIntercepted requestAccessToken,
			ILogger<DatasetHttpQuery> logger,
			JsonHandlingService jsonHandlingService,
			ErrorThesaurus errors)
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
		}

		public DatasetHttpQuery Ids(IEnumerable<Guid> ids) { this._ids = ids?.ToList(); return this; }
		public DatasetHttpQuery Ids(Guid id) { this._ids = id.AsList(); return this; }
		public DatasetHttpQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public DatasetHttpQuery ExcludedIds(Guid excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public DatasetHttpQuery CollectionIds(IEnumerable<Guid> collectionIds) { this._collectionIds = collectionIds?.ToList(); return this; }
		public DatasetHttpQuery CollectionIds(Guid collectionId) { this._collectionIds = collectionId.AsList(); return this; }
		public DatasetHttpQuery Like(String like) { this._like = like; return this; }
		public DatasetHttpQuery Properties(IEnumerable<string> properties) { this._properties = properties?.ToList(); return this; }
		public DatasetHttpQuery Properties(string property) { this._properties = property.AsList(); return this; }
		public DatasetHttpQuery Types(IEnumerable<string> types) { this._types = types?.ToList(); return this; }
		public DatasetHttpQuery Types(string type) { this._types = type.AsList(); return this; }
		public DatasetHttpQuery State(Common.Enum.DatasetState state) { this._datasetStatus = state; return this; }
		public DatasetHttpQuery PublishedDateFrom(DateTime date) { this._publishedDateFrom = date; return this; }
		public DatasetHttpQuery PublishedDateTo(DateTime date) { this._publishedDateTo = date; return this; }


		protected bool IsFalseQuery()
		{
			return this._ids.IsNotNullButEmpty() || this._excludedIds.IsNotNullButEmpty() || this._collectionIds.IsNotNullButEmpty() || this._properties.IsNotNullButEmpty() || this._types.IsNotNullButEmpty();
		}

		public async Task<List<Dataset>> CollectAsync()
		{
			DatasetQueryList collectedItems = await CollectBaseAsync(false);
			return collectedItems == null || collectedItems.Datasets == null ? null : collectedItems.Datasets.Select(x => new Dataset
			{
				Country = [x.Country],
				DatePublished = x.DatePublished == null ? null : DateOnly.FromDateTime(x.DatePublished.Value),
				Description = x.Description,
				FieldOfScience = x.FieldsOfScience,
				Headline = x.Headline,
				Id = Guid.TryParse(x.Id, out Guid parsedId) ? parsedId : Guid.Empty,
				Keywords = x.Keywords,
				License = x.License,
				Name = x.Name,
				Url = x.Url,
				Version = x.Version,
				Language = x.Languages,
			}).ToList();
		}

		public async Task<DatasetQueryList> CollectBaseAsync(bool useInCount)
		{
			//string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			//if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			QueryString qs = new QueryString();
			if (this._ids != null) this._ids.ForEach(x => qs = qs.Add("nodeIds", x.ToString()));
			if (this._properties != null) this._properties.ForEach(x => qs = qs.Add("properties", x));
			if (this._types != null) this._types.ForEach(x => qs = qs.Add("types", x));
			if (this.Order != null && !this.Order.IsEmpty)
			{
				if (this.Order.Items != null)
				{
					this.Order.Items.Select(x => qs = qs.Add("orderBy", new OrderingFieldResolver(x).Field));
					qs = qs.Add("direction", new OrderingFieldResolver(this.Order.Items.FirstOrDefault()).IsAscending ? "1" : "-1");
				}
			}
			if (this._publishedDateFrom != null) qs = qs.Add("publishedDateFrom", this._publishedDateFrom.Value.ToString("yyyy-MM-dd"));
			if (this._publishedDateTo != null) qs = qs.Add("publishedDateTo", this._publishedDateTo.Value.ToString("yyyy-MM-dd"));
			if (this._datasetStatus != null) qs = qs.Add("dataset_status", this._datasetStatus.Value.ToString());

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DatasetQueryEndpoint}{qs.ToString()}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			//request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				DatasetQueryList model = this._jsonHandlingService.FromJson<DatasetQueryList>(content);
				if (model.Code != 200) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, model.Message, null, UnderpinningServiceType.DataManagement, this._logCorrelationScope.CorrelationId);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DataManagement, this._logCorrelationScope.CorrelationId);
			}
		}

		public async Task<int> CountAsync()
		{
			//String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			//if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			//TODO: Apply query
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DatasetQueryEndpoint}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			//request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");
			request.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			String content = await this.SendRequest(request);
			try
			{
				int count = Convert.ToInt32(content);
				return count;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.DataManagement, this._logCorrelationScope.CorrelationId);
			}
		}

		private async Task<String> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.DataManagement, this._logCorrelationScope.CorrelationId);
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
			return content;
		}
	}
}
