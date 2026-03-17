using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Service.Discovery.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator
{
	public class TaskOrchestratorService : ITaskOrchestratorService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly TaskOrchestratorHttpConfig _config;
		private readonly LogTrackingCorrelationConfig _logTrackingCorrelationConfig;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly ILogger<TaskOrchestratorService> _logger;
		private readonly RequestTokenIntercepted _requestAccessToken;
		private readonly QueryFactory _queryFactory;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;
		private readonly AnalyticalPatternTemplates _analyticalPatternTemplates;


		public TaskOrchestratorService(IAccessTokenService accessTokenService,
		IHttpClientFactory httpClientFactory,
		TaskOrchestratorHttpConfig config,
		LogTrackingCorrelationConfig logTrackingCorrelationConfig,
		LogCorrelationScope logCorrelationScope,
		ILogger<TaskOrchestratorService> logger,
		RequestTokenIntercepted requestAccessToken,
		QueryFactory queryFactory,
		ErrorThesaurus errors,
		JsonHandlingService jsonHandlingService,
		BuilderFactory builderFactory,
		AnalyticalPatternTemplates analyticalPatternTemplates)
		{
			this._accessTokenService = accessTokenService;
			this._httpClientFactory = httpClientFactory;
			this._config = config;
			this._logTrackingCorrelationConfig = logTrackingCorrelationConfig;
			this._logCorrelationScope = logCorrelationScope;
			this._logger = logger;
			this._queryFactory = queryFactory;
			this._errors = errors;
			this._jsonHandlingService = jsonHandlingService;
			this._builderFactory = builderFactory;
			this._requestAccessToken = requestAccessToken;
			this._analyticalPatternTemplates = analyticalPatternTemplates;
		}

		public async Task<IEnumerable<CrossDatasetDiscoveryResult>> CrossDatasetDiscoverySearch(string query)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);
			var apRequest = this._analyticalPatternTemplates.CrossDatasetDiscoveryLookup
				.Replace("{{AP_node_Id}}", Guid.NewGuid().ToString())
				.Replace("{{Op_node_Id}}", Guid.NewGuid().ToString())
				.Replace("{{File_Obj_node_Id}}", Guid.NewGuid().ToString())
				.Replace("{{Task_node_Id}}", Guid.NewGuid().ToString())
				.Replace("{{User_node_Id}}", Guid.NewGuid().ToString())
				.Replace("{{start_time}}", DateTime.UtcNow.ToString("O"))
				.Replace("{{query}}", query);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.CrossDatasetDiscoverySearchEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(apRequest), Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			String content = await this.SendRequest(httpRequest);
			JObject json = JObject.Parse(content);
			return json["content"]?["metadata"]?["results"]?.ToObject<IEnumerable<CrossDatasetDiscoveryResult>>();
		}

		private async Task<string> SendRequest(HttpRequestMessage request)
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
