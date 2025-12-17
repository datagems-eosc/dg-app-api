using Cite.Tools.Data.Censor;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Conversation;
using DataGEMS.Gateway.App.Service.Discovery;
using DataGEMS.Gateway.App.Service.InDataExploration;
using DataGEMS.Gateway.App.Service.QueryRecommender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
		private readonly ILogger<SearchController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;
		private readonly IConversationService _conversationService;
		private readonly IQueryRecommenderHttpService _queryRecommenderHttpService;

		public SearchController(
			CensorFactory censorFactory,
			ICrossDatasetDiscoveryService crossDatasetDiscoveryService,
			IInDataExplorationService inDataExplorationService,
			IAccountingService accountingService,
			ILogger<SearchController> logger,
			IConversationService conversationService,
			ErrorThesaurus errors,
			IQueryRecommenderHttpService queryRecommenderHttpService)
		{
			this._censorFactory = censorFactory;
			this._crossDatasetDiscoveryService = crossDatasetDiscoveryService;
			this._inDataExplorationService = inDataExplorationService;
			this._accountingService = accountingService;
			this._conversationService = conversationService;
			this._logger = logger;
			this._errors = errors;
			this._queryRecommenderHttpService = queryRecommenderHttpService;
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
				UserCollectionIds = lookup.UserCollectionIds,
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
			this._logger.Debug(new MapLogEntry("explore query exploring").And("type", nameof(App.Model.InDataExplore)).And("lookup", lookup));

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
			[SwaggerRequestBody(description: "The recommendation query", Required = true)]
			QueryRecommendationLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("recommend query exploring").And("type", nameof(App.Model.QueryRecommendation)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<QueryRecommenderCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			RecommenderInfo request = new RecommenderInfo()
			{
				Query = lookup.Query,
			};

			List<App.Model.QueryRecommendation> results = await this._queryRecommenderHttpService.RecommendAsync(request, censoredFields);

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
	}
}
