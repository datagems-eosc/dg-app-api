using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Discovery.Model;
using DataGEMS.Gateway.App.Service.TaskOrchestrator.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
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
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly Data.AppDbContext _dbContext;
		private readonly EventBroker _eventBroker;


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
		AnalyticalPatternTemplates analyticalPatternTemplates,
		IAuthorizationContentResolver authorizationContentResolver,
		Data.AppDbContext dbContext,
		EventBroker eventBroker)
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
			this._authorizationContentResolver = authorizationContentResolver;
			this._dbContext = dbContext;
			this._eventBroker = eventBroker;
		}

		public async Task<AdHocQuery> AdHocQueryAsync(AdHocQueryPersist persist, IFieldSet fields = null)
		{
			string token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);
			
			var apRequest = new 
			{ 
				ap = BuildAdHocAnalyticalPattern(persist)
			};
			string apRequestJson = JsonConvert.SerializeObject(apRequest, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
			});
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.AdHocQueryEndpoint}")
			{
				Content = new StringContent(apRequestJson, Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);
			string content = await this.SendRequest(httpRequest, this._httpClientFactory.CreateClient("AdHocQueryClient"));
			AnalyticalPattern deserializedResponse = this._jsonHandlingService.FromJsonSafe<AdHocQueryTaskOrchestratorResponse>(content).ap;
			DateTime now = DateTime.UtcNow;
			
			string resultFilePath = deserializedResponse?.Nodes?.FirstOrDefault(x => 
				x.Labels != null && 
				x.Properties != null && 
				x.Properties.ContainsKey("contentUrl") && 
				x.Labels.Contains("cr:FileObject") && 
				x.Labels.Contains("Data"))?.Properties["contentUrl"].ToString();
			Guid userId = (await this._authorizationContentResolver.CurrentUserId()).Value;

			var data = new AdHocQueryResult
			{
				AnalyticalPattern = content,
				ResultFilePath = resultFilePath,
				CreatedAt = now,
				Id = Guid.NewGuid(),
				UserId = userId,
				IsActive = IsActive.Active,
				UpdatedAt = now,
			};

			this._dbContext.Add(data);
			await _dbContext.SaveChangesAsync();

			this._eventBroker.EmitAdHocQueryResultTouched(data.Id);

			//App.Model.AdHocQuery model = await _builderFactory.Builder<App.Model.Builder.AdHocQueryBuilder>().Build(FieldSet.Build(fields, nameof(App.Model.AdHocQuery.Id)), data);
			return new AdHocQuery();
		}

		

		public async Task<IEnumerable<CrossDatasetDiscoveryResult>> CrossDatasetDiscoverySearch(Model.CrossDatasetDiscoveryRequest request)
		{
			String token = await this._accessTokenService.GetExchangeAccessTokenAsync(this._requestAccessToken.AccessToken, this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);
			var apRequest = this._analyticalPatternTemplates.CrossDatasetDiscoveryLookup
				.Replace("{{ap_node_id}}", Guid.NewGuid().ToString())
				.Replace("{{op_node_id}}", Guid.NewGuid().ToString())
				.Replace("{{file_obj_node_id}}", Guid.NewGuid().ToString())
				.Replace("{{task_node_id}}", Guid.NewGuid().ToString())
				.Replace("{{user_node_id}}", Guid.NewGuid().ToString())
				.Replace("{{start_time}}", DateTime.UtcNow.ToString("O"))
				.Replace("{{query}}", request.Query)
				.Replace("{{k}}", request.ResultCount.ToString());

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.CrossDatasetDiscoverySearchEndpoint}")
			{
				Content = new StringContent(apRequest, Encoding.UTF8, "application/json")
			};
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpRequest.Headers.Add(this._logTrackingCorrelationConfig.HeaderName, this._logCorrelationScope.CorrelationId);

			String content = await this.SendRequest(httpRequest);
			JObject json = JObject.Parse(content);
			return json["content"]?["metadata"]?["results"]?.ToObject<IEnumerable<CrossDatasetDiscoveryResult>>();
		}

		private async Task<string> SendRequest(HttpRequestMessage request, HttpClient client = null)
		{
			HttpResponseMessage response = null;
			try { 
				var chosenClient = client ?? this._httpClientFactory.CreateClient();
				response = await chosenClient.SendAsync(request);
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.TaskOrchestrator, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.TaskOrchestrator, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			String content = await response.Content.ReadAsStringAsync();
			return content;
		}

		private static AnalyticalPattern BuildAdHocAnalyticalPattern(AdHocQueryPersist persist)
		{
			DateTime now = DateTime.UtcNow;
			AnalyticalPatternNode analyticalPatternNode = new AnalyticalPatternNode
			{
				Id = Guid.NewGuid(),
				Labels = ["Analytical_Pattern"],
				Properties = new Dictionary<string, object>
				{
					{ "description", "Analytical Pattern to query a dataset" },
					{ "name", "Query Dataset AP" },
					{ "process", "query" },
					{ "startTime", now.ToString("O") }
				}
			};
			AnalyticalPatternNode sqlOperatorNode = new AnalyticalPatternNode
			{
				Id = Guid.NewGuid(),
				Labels = ["SQL_Operator", "Query_Operator"],
				Properties = new Dictionary<string, object>
				{
					{"description", "Query executed via DuckDB on Postgres" },
					{  "name", "DuckDB Query Operator" },
					{  "query", persist.Query },
					{  "queryType", "SELECT" },
					{  "startTime", now.ToString("O") }
				}
			};
			AnalyticalPatternNode datasetNode = new AnalyticalPatternNode
			{
				Id = persist.DatasetId.Value,
				Labels = ["sc:Dataset"]
			};
			AnalyticalPatternNode outputNode = new AnalyticalPatternNode
			{
				Id = Guid.NewGuid(),
				Labels = ["cr:FileObject", "Data"]
			};
			AnalyticalPatternNode databaseConnectionNode = new AnalyticalPatternNode
			{
				Id = persist.DatabaseConnectionId.Value,
				Labels = ["dg:DatabaseConnection"]
			};
			var argumentsNodes = persist.Arguments?.Select(x => new AnalyticalPatternNode
			{
				Id = x.Key,
				Labels = ["cr:FileObject"],
			});
			AnalyticalPatternNode userNode = new AnalyticalPatternNode
			{
				Id = Guid.NewGuid(),
				Labels = ["User"]
			};
			AnalyticalPatternNode taskNode = new AnalyticalPatternNode
			{
				Id = Guid.NewGuid(),
				Labels = ["Task"],
				Properties = new Dictionary<string, object>
				{
					{ "description", "Task to query a dataset" },
					{ "name", "Dataset Querying Task" },
				}
			};
			AnalyticalPatternEdge consistEdge = new AnalyticalPatternEdge
			{
				From = analyticalPatternNode.Id,
				To = sqlOperatorNode.Id,
				Labels = ["consist_of"]
			};
			IEnumerable<AnalyticalPatternEdge> inputEdges = persist.Arguments?.Select(x => new AnalyticalPatternEdge
			{
				From = x.Key,
				To = sqlOperatorNode.Id,
				Labels = ["input"],
				Properties = new Dictionary<string, object>
				{
					{ "argname", x.Value }
				}
			});
			IEnumerable<AnalyticalPatternEdge> containedEdges = argumentsNodes?.Select(x => new AnalyticalPatternEdge
			{
				From = x.Id,
				To = databaseConnectionNode.Id,
				Labels = ["contained_in"]
			});
			AnalyticalPatternEdge distributionEdge = new AnalyticalPatternEdge
			{
				From = datasetNode.Id,
				To = databaseConnectionNode.Id,
				Labels = ["distribution"]
			};
			IEnumerable<AnalyticalPatternEdge> distributionEdges = argumentsNodes?.Select(x => new AnalyticalPatternEdge
			{
				From = datasetNode.Id,
				To = x.Id,
				Labels = ["distribution"]
			});
			AnalyticalPatternEdge outputEdge = new AnalyticalPatternEdge
			{
				From = sqlOperatorNode.Id,
				To = outputNode.Id,
				Labels = ["output"]
			};
			AnalyticalPatternEdge accomplishedEdge = new AnalyticalPatternEdge
			{
				From = taskNode.Id,
				To = analyticalPatternNode.Id,
				Labels = ["is_accomplished"]
			};
			AnalyticalPatternEdge requestEdge = new AnalyticalPatternEdge
			{
				From = userNode.Id,
				To = taskNode.Id,
				Labels = ["request"]
			};
			AnalyticalPattern ap = new AnalyticalPattern
			{
				Nodes = [analyticalPatternNode, sqlOperatorNode, datasetNode, outputNode, databaseConnectionNode, userNode, taskNode],
				Edges = [consistEdge, outputEdge, distributionEdge, accomplishedEdge, requestEdge]
			};
			if (persist.Arguments != null && persist.Arguments.Count > 0)
			{
				ap.Nodes.AddRange(argumentsNodes);
				ap.Edges.AddRange(inputEdges);
				ap.Edges.AddRange(containedEdges);
				ap.Edges.AddRange(distributionEdges);
			}

			return ap;
		}
	}

}
