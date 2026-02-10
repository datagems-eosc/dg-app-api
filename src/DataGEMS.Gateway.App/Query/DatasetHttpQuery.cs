using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataGEMS.Gateway.App.Query
{
	public class DatasetHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _collectionIds { get; set; }
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
		public DatasetHttpQuery Types(IEnumerable<string> types) { this._types = types?.ToList(); return this; }
		public DatasetHttpQuery Types(string type) { this._types = type.AsList(); return this; }
		public DatasetHttpQuery State(Common.Enum.DatasetState state) { this._datasetStatus = state; return this; }
		public DatasetHttpQuery PublishedDateFrom(DateTime date) { this._publishedDateFrom = date; return this; }
		public DatasetHttpQuery PublishedDateTo(DateTime date) { this._publishedDateTo = date; return this; }


		protected bool IsFalseQuery()
		{
			return this._ids.IsNotNullButEmpty() || this._excludedIds.IsNotNullButEmpty() || this._collectionIds.IsNotNullButEmpty() || this._types.IsNotNullButEmpty();
		}

		public async Task<List<Dataset>> CollectAsync()
		{
			return await this.CollectAsync(null);
		}

		public async Task<List<Dataset>> CollectAsync(IFieldSet projection)
		{
			DatasetQueryList collectedItems = await this.CollectBaseAsync(false, projection);
			if (collectedItems == null || collectedItems.Datasets == null) return null;

			return collectedItems.Datasets
				.SelectMany(x => x.Nodes)
				.Where(x => x.ContainsKey("labels") && ((JArray)x["labels"]).ToList().Contains("sc:Dataset"))
				.Select(x =>
				{
					JObject properties = x.ContainsKey("properties") && x["properties"] != null ? (JObject)x["properties"] : null;
					if (properties == null)
					{
						return null;
					}
					return new Dataset
					{
						Id = properties.ContainsKey("id") && properties["id"] != null ? (Guid)properties["id"] : Guid.Empty,
						Name = properties.ContainsKey("name") ? (string)properties["name"] : null,
						ArchivedAt = properties.ContainsKey("sc:archivedAt") ? (string)properties["sc:archivedAt"] : null,
						Description = properties.ContainsKey("description") ? (string)properties["description"] : null,
						ConformsTo = properties.ContainsKey("conformsTo") ? (string)properties["conformsTo"] : null,
						CiteAs = properties.ContainsKey("citeAs") ? (string)properties["citeAs"] : null,
						License = properties.ContainsKey("license") ? (string)properties["license"] : null,
						Url = properties.ContainsKey("url") ? (string)properties["url"] : null,
						Version = properties.ContainsKey("version") ? (string)properties["version"] : null,
						Headline = properties.ContainsKey("dg:headline") ? (string)properties["dg:headline"] : null,
						Keywords = properties.ContainsKey("dg:keywords") && properties["dg:keywords"] != null ? ((JArray)properties["dg:keywords"]).ToObject<List<string>>() : null,
						FieldOfScience = properties.ContainsKey("dg:fieldOfScience") && properties["dg:fieldOfScience"] != null ? ((JArray)properties["dg:fieldOfScience"]).ToObject<List<string>>() : null,
						Language = properties.ContainsKey("inLanguage") && properties["inLanguage"] != null ? ((JArray)properties["inLanguage"]).ToObject<List<string>>() : null,
						Country = properties.ContainsKey("country") ? [(string)properties["country"]] : null,
						DatePublished = properties.ContainsKey("datePublished") && properties["datePublished"] != null ? DateOnly.FromDateTime((DateTime)properties["datePublished"]) : null,
						Status = properties.ContainsKey("dg:status") ? (string)properties["dg:status"] : null,
						Code = properties.ContainsKey("code") ? (string)properties["code"] : null,
						Size = properties.ContainsKey("size") && properties["size"] != null ? (long?)properties["size"] : null,
						MimeType = properties.ContainsKey("mime_type") ? (string)properties["mime_type"] : null,
						//TODO: Access
						//TODO: UploadedBy
						//TODO: Distribution
						//TODO: RecordSet
						//TODO: Type
						//TODO: code
						//TODO: size
						//TODO: mime_type

					};
				}).ToList();
		}

		public async Task<DatasetQueryList> CollectBaseAsync(bool useInCount, IFieldSet projection)
		{
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			QueryString qs = this.CreateFilterQuery();
			qs = this.BuildProjection(qs, projection);
			if (this.Order != null && !this.Order.IsEmpty)
			{
				this.Order.Items.Select(x => qs = qs.Add("orderBy", new OrderingFieldResolver(x).Field));
				qs = qs.Add("direction", new OrderingFieldResolver(this.Order.Items.FirstOrDefault()).IsAscending ? "1" : "-1");
			}

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DatasetQueryEndpoint}{qs.ToString()}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");
			request.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

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
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			QueryString qs = this.CreateFilterQuery();

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DatasetQueryEndpoint}{qs.ToString()}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");
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

		private QueryString CreateFilterQuery()
		{
			QueryString qs = new QueryString();
			if (this._ids != null) this._ids.ForEach(x => qs = qs.Add("nodeIds", x.ToString()));
			if (this._types != null) this._types.ForEach(x => qs = qs.Add("types", x));
			if (this._publishedDateFrom != null) qs = qs.Add("publishedDateFrom", this._publishedDateFrom.Value.ToString("yyyy-MM-dd"));
			if (this._publishedDateTo != null) qs = qs.Add("publishedDateTo", this._publishedDateTo.Value.ToString("yyyy-MM-dd"));
			if (this._datasetStatus != null) qs = qs.Add("dataset_status", this._datasetStatus.Value.ToString().ToLower());
			return qs;
		}

		private QueryString BuildProjection(QueryString qs, IFieldSet projection)
		{
			if (projection == null || projection.IsEmpty()) return qs;

			List<String> fields = new List<string>();
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("headline");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("fieldOfScience");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("name");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("conformsTo");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("url");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("datePublished");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("license");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("keywords");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("description");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("inLanguage");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("version");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("archivedAt");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("citeAs");
			if (projection.HasField(nameof(Model.Dataset.Country))) fields.Add("country");

			fields.ForEach(x => qs = qs.Add("properties", x));

			return qs;
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
