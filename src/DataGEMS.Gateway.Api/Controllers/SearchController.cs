using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.Api.OpenApi;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Model.Builder;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.Conversation;
using DataGEMS.Gateway.App.Service.DatasetPackaging;
using DataGEMS.Gateway.App.Service.DatasetRecommender;
using DataGEMS.Gateway.App.Service.Discovery;
using DataGEMS.Gateway.App.Service.InDataExploration;
using DataGEMS.Gateway.App.Service.QueryRecommender;
using DataGEMS.Gateway.App.Service.TaskOrchestrator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/search")]
	[ApiController]
	public class SearchController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly ICrossDatasetDiscoveryService _crossDatasetDiscoveryService;
		private readonly IInDataExplorationService _inDataExplorationService;
		private readonly IQueryRecommenderService _queryRecommenderService;
		private readonly ILogger<SearchController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;
		private readonly IConversationService _conversationService;
		private readonly ITaskOrchestratorService _taskOrchestratorService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
		private readonly BuilderFactory _builderFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IDatasetPackagingService _packagingService;
		private readonly IDatasetRecommenderService _datasetRecommenderService;

		public SearchController(
			CensorFactory censorFactory,
			ICrossDatasetDiscoveryService crossDatasetDiscoveryService,
			IInDataExplorationService inDataExplorationService,
			IQueryRecommenderService queryRecommenderService,
			IAccountingService accountingService,
			ILogger<SearchController> logger,
			IConversationService conversationService,
			ErrorThesaurus errors,
			ITaskOrchestratorService taskOrchestratorService,
			IAuthorizationContentResolver authorizationContentResolver,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			BuilderFactory builderFactory,
			QueryFactory queryFactory,
			IDatasetPackagingService datasetPackagingService,
			IDatasetRecommenderService datasetRecommenderService)
		{
			this._censorFactory = censorFactory;
			this._crossDatasetDiscoveryService = crossDatasetDiscoveryService;
			this._inDataExplorationService = inDataExplorationService;
			this._queryRecommenderService = queryRecommenderService;
			this._accountingService = accountingService;
			this._conversationService = conversationService;
			this._logger = logger;
			this._errors = errors;
			this._taskOrchestratorService = taskOrchestratorService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._localizer = localizer;
			this._builderFactory = builderFactory;
			this._queryFactory = queryFactory;
			this._packagingService = datasetPackagingService;
			this._datasetRecommenderService = datasetRecommenderService;
		}

		[HttpPost("cross-dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(CrossDatasetDiscoveryLookup.CrossDatasetDiscoveryLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Cross-dataset search")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.CrossDatasetDiscovery>>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<List<App.Model.CrossDatasetDiscovery>>> CrossDatasetDiscoveryAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The discovery query", Required = true)]
			CrossDatasetDiscoveryLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("cross dataset discovering").And("type", nameof(App.Model.CrossDatasetDiscovery)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<CrossDatasetDiscoveryCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DiscoverInfo request = new DiscoverInfo()
			{
				Query = lookup.Query,
				ResultCount = lookup.ResultCount,
				DatasetIds = lookup.DatasetIds,
				CollectionIds = lookup.CollectionIds,
			};

			List<CrossDatasetDiscovery> results = await this._crossDatasetDiscoveryService.DiscoverAsync(request, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.CrossDatasetDiscovery.AsAccountable());

			Guid? conversationId = await this.UpdateConversation(
				lookup.ConversationOptions?.ConversationId,
				lookup.ConversationOptions?.AutoCreateConversation,
				lookup.Query,
				null,
				new App.Common.Conversation.CrossDatasetQueryConversationEntry()
				{
					Version = DiscoverInfo.ModelVersion,
					Payload = request
				},
				new App.Common.Conversation.CrossDatasetResponseConversationEntry()
				{
					Version = CrossDatasetDiscovery.ModelVersion,
					Payload = results
				});

			return new SearchResult<List<CrossDatasetDiscovery>>(conversationId, results);
		}

		[HttpPost("in-data-explore")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(InDataExplorationLookup.InDataExplorationLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Explore in selected data")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.InDataExplore>>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<App.Model.InDataExplore>> SimpleExploreAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The exploration query", Required = true)]
			InDataExplorationLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("in data exploration").And("type", nameof(App.Model.InDataExplore)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<InDataExplorationCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ExploreInfo request = new ExploreInfo()
			{
				Question = lookup.Query,
				DatasetIds = lookup.DatasetIds,
			};

			App.Model.InDataExplore results = await this._inDataExplorationService.ExploreAsync(request, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.InDataExploration.AsAccountable());


			Guid? conversationId = await this.UpdateConversation(
				lookup.ConversationOptions?.ConversationId,
				lookup.ConversationOptions?.AutoCreateConversation,
				lookup.Query,
				null,
				new App.Common.Conversation.InDataExploreQueryConversationEntry()
				{
					Version = ExploreInfo.ModelVersion,
					Payload = request
				},
				new App.Common.Conversation.InDataSimpleExploreResponseConversationEntry()
				{
					Version = App.Model.InDataExplore.ModelVersion,
					Payload = results
				});

			return new SearchResult<App.Model.InDataExplore>(conversationId, results);
		}

		[HttpPost("disambiguate-query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(QueryDisambiguationLookup.QueryDisambiguationLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Disambiguates the provided query by resolving ambiguous terms or intent and returns clearer, more specific versions")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<App.Model.QueryDisambiguationViewModel>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<App.Model.QueryDisambiguationViewModel>> DisambiguateQueryAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The query disambiguation options", Required = true)]
			QueryDisambiguationLookup lookup
		)
		{
			this._logger.Debug(new MapLogEntry("query disambiguation").And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<QueryDisambiguationCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DisambiguationInfo info = new DisambiguationInfo
			{
				DatasetIds = lookup.DatasetIds,
				Query = lookup.Query,
			};

			App.Model.QueryDisambiguationViewModel results = await this._taskOrchestratorService.QueryDisambiguationAsync(info, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.QueryDisambiguation.AsAccountable());

			Guid? conversationId = await this.UpdateConversation(
				lookup.ConversationOptions?.ConversationId,
				lookup.ConversationOptions?.AutoCreateConversation,
				lookup.Query,
				null,
				new App.Common.Conversation.QueryDisambiguationQueryConversationEntry()
				{
					Version = ExploreInfo.ModelVersion,
					Payload = info
				},
				new App.Common.Conversation.QueryDisambiguationResponseConversationEntry()
				{
					Version = App.Model.QueryDisambiguation.ModelVersion,
					Payload = results
				});

			return new SearchResult<App.Model.QueryDisambiguationViewModel>(conversationId, results);
		}


		[HttpPost("recommend")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(QueryRecommendationLookup.QueryRecommendationLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Recommend possible queries")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.QueryRecommendation>>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<List<App.Model.QueryRecommendation>>> RecommendAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The query recommendation options", Required = true)]
			QueryRecommendationLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("query recommendation").And("type", nameof(App.Model.QueryRecommendation)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<QueryRecommenderCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			RecommenderInfo request = new RecommenderInfo()
			{
				Query = lookup.Query,
			};

			List<App.Model.QueryRecommendation> results = await this._queryRecommenderService.RecommendAsync(request, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.QueryRecommender.AsAccountable());

			Guid? conversationId = await this.UpdateConversation(
				lookup.ConversationOptions?.ConversationId,
				lookup.ConversationOptions?.AutoCreateConversation,
				lookup.Query,
				null,
				new App.Common.Conversation.QueryRecommenderQueryConversationEntry()
				{
					Version = ExploreInfo.ModelVersion,
					Payload = request
				},
				new App.Common.Conversation.QueryRecommenderResponseConversationEntry()
				{
					Version = App.Model.QueryRecommendation.ModelVersion,
					Payload = results
				});

			return new SearchResult<List<App.Model.QueryRecommendation>>(conversationId, results);
		}

		private async Task<Guid?> UpdateConversation(Guid? conversationId, Boolean? autoCreateConversation, String currentQuery, IEnumerable<Guid> datasetIds, params App.Common.Conversation.ConversationEntry[] entries)
		{
			if (!conversationId.HasValue && (!autoCreateConversation.HasValue || (autoCreateConversation.HasValue && !autoCreateConversation.Value))) return null;

			if (!conversationId.HasValue)
			{
				String conversationName = await this._conversationService.GenerateConversationName(conversationId, currentQuery);
				Conversation model = await this._conversationService.PersistAsync(new ConversationPersist() { Name = conversationName }, new FieldSet(nameof(Conversation.Id)));
				if (model.Id.HasValue) conversationId = model.Id.Value;
			}
			if (!conversationId.HasValue) return null;

			await this._conversationService.AppendToConversation(conversationId.Value, entries);
			await this._conversationService.SetConversationDatasets(conversationId.Value, datasetIds);
			return conversationId.Value;
		}

		[HttpGet("dataset/{datasetId}/recommend")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Recommend possible datasets")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(List<App.Model.Dataset>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Model.Dataset>> RecommendDatasetsAsync(
			[FromRoute]
			[SwaggerRequestBody(description: "The dataset id", Required = true)]
			Guid datasetId,

			[FromQuery]
			[SwaggerRequestBody(description: "The number of recommendations to return", Required = false)]
			int? n,
			
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("dataset recommendation").And("type", nameof(App.Model.Dataset)).And("datasetId", datasetId).And("recommendationsCount", n));

			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			int finalN = n.HasValue ? n.Value : 2;
			if (n <= 0) throw new DGValidationException(this._errors.InvalidValue.Code, string.Format(this._errors.InvalidValue.Message, nameof(n)));
			List<Guid> response = await this._taskOrchestratorService.DatasetRecommendationAsync(datasetId, finalN);

			DatasetHttpQuery query = this._queryFactory.Query<DatasetHttpQuery>().Ids(response);
			DatasetHttpQuery.QueryResult results = await query.CollectAsync();
			List<Dataset> models = await this._builderFactory.Builder<DatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, results.Items);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.DatasetRecommender.AsAccountable());

			return models;
		}

		[HttpPost("ad-hoc/evaluate")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(AdHocQueryEvaluate.EvaluateValidator), "query")]
		[SwaggerOperation(Summary = "Execute an ad-hoc query")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.AdHocQuery>>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.AdHocQuery> AdHocQueryAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The ad-hoc query", Required = true)]
			AdHocQueryEvaluate query,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("Ad-hoc query").And("type", nameof(App.Model.AdHocQuery)).And("query", query).And("fields", fieldSet));
			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);
			IFieldSet censoredFields = await this._censorFactory.Censor<AdHocQueryCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			var results = await this._taskOrchestratorService.AdHocQueryAsync(query, fieldSet);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.AdHocQuery.AsAccountable());

			return results;
		}

		[HttpPost("ad-hoc/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(AdHocQueryLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query ad-hoc queries")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching ad-hoc query results along with the count", type: typeof(QueryResult<App.Model.AdHocQuery>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.AdHocQuery>> Query(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			AdHocQueryLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.AdHocQuery)).And("lookup", lookup));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);
			IFieldSet censoredFields = await this._censorFactory.Censor<AdHocQueryCensor>().Censor(lookup.Project, CensorContext.AsCensor(), userId);
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			AdHocQueryQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<App.Data.AdHocQueryResult> datas = await query.CollectAsync();
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : datas.Count;
			List<App.Model.AdHocQuery> models = await this._builderFactory.Builder<AdHocQueryBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.AdHocQuery.AsAccountable());
			return new QueryResult<App.Model.AdHocQuery>(models, count);
		}

		[HttpGet("ad-hoc/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve ad-hoc query results")]
		[SwaggerResponse(statusCode: 200, description: "The result", type: typeof(AdHocQuery))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<AdHocQuery> GetAdHocResult(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.AdHocQuery)).And("id", id).And("fields", fieldSet));
			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);
			IFieldSet censoredFields = await this._censorFactory.Censor<AdHocQueryCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			AdHocQueryQuery query = this._queryFactory.Query<AdHocQueryQuery>().Ids(id).DisableTracking().Authorize(AuthorizationFlags.Any);
			App.Data.AdHocQueryResult data = await query.FirstAsync();
			App.Model.AdHocQuery model = await this._builderFactory.Builder<AdHocQueryBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.AdHocQuery)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.AdHocQuery.AsAccountable());

			return model;
		}

		[HttpGet("ad-hoc/{id}/preview/{lines}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve ad-hoc query results preview")]
		[SwaggerResponse(statusCode: 200, description: "The result", type: typeof(string))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<string> GetAdHocPreview(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,

			[FromRoute]
			[SwaggerParameter(description: "The number of lines to include in the preview", Required = true)]
			uint lines)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.AdHocQuery)).And("id", id).And("lines", lines));
			
			string model = await this._taskOrchestratorService.AdHocQueryPreviewAsync(id, (int)lines);

			this._accountingService.AccountFor(KnownActions.Preview, KnownResources.AdHocQuery.AsAccountable());

			return model;
		}

		[HttpPost("package/recommend")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(PackageRecommendationLookup.PackageRecommendationLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Recommend possible packages")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.PackageRecommendation>>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.PackageRecommendation> PackageRecommendAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The package recommendation options", Required = true)]
			PackageRecommendationLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("package recommendation").And("type", nameof(App.Model.PackageRecommendation)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<PackageRecommenderCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			PackageRecommendation results = await this._packagingService.RecommendAsync(new PackageRecommendationRequest
			{
				DatasetIds = lookup.DatasetIds,
				DatasetsPerPackage = lookup.DatasetsPerPackage,
				PackagesCount = lookup.PackagesCount,
			}, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.DatasetPackaging.AsAccountable());

			return results;
		}
	}
}
