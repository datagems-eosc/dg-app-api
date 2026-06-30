using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common.Conversation;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Conversation;
using DataGEMS.Gateway.App.Service.DataManagement;
using DataGEMS.Gateway.App.Service.DatasetRecommender;
using DataGEMS.Gateway.App.Service.Discovery;
using DataGEMS.Gateway.App.Service.Discovery.Model;
using DataGEMS.Gateway.App.Service.InDataExploration;
using DataGEMS.Gateway.App.Service.InDataExploration.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/pilot")]
	[ApiController]
	public class PilotController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<PilotController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly IDataManagementService _datasetService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
		private readonly IDatasetRecommenderService _datasetRecommenderService;
		private readonly ICrossDatasetDiscoveryService _crossDatasetDiscoveryService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IInDataExplorationService _inDatasetDiscoveryService;
		private readonly IConversationService _conversationService;

		public PilotController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<PilotController> logger,
			IAccountingService accountingService,
			IDataManagementService datasetService,
			ErrorThesaurus errors,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			IDatasetRecommenderService datasetRecommenderService,
			ICrossDatasetDiscoveryService crossDatasetDiscoveryService,
			IAuthorizationContentResolver authorizationContentResolver,
			IInDataExplorationService inDatasetDiscoveryService,
			IConversationService conversationService)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._datasetService = datasetService;
			this._errors = errors;
			this._localizer = localizer;
			this._datasetRecommenderService = datasetRecommenderService;
			this._crossDatasetDiscoveryService = crossDatasetDiscoveryService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._inDatasetDiscoveryService = inDatasetDiscoveryService;
			this._conversationService = conversationService;
		}

		[HttpPost("mathe/recommend")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(MatheRecommendationLookup.RequestValidator), "lookup")]
		[SwaggerOperation(Summary = "Generate material-based recommendations")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<App.Service.DatasetRecommender.Model.MatheRecommendationResponse>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<App.Service.DatasetRecommender.Model.MatheRecommendationResponse>> RecommendDatasetsAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The field set to apply for building the results", Required = true)]
			MatheRecommendationLookup lookup
			)
		{
			this._logger.Debug(new MapLogEntry("mathE recommendation").And("type", nameof(App.Model.Dataset)).And("request", lookup));
			var serviceRequest = new App.Service.DatasetRecommender.Model.MatheRecommendationRequest
			{
				QuestionId = lookup.QuestionId,
				Question = lookup.Question,
				RecommendedMaterialsCount = lookup.RecommendedMaterialsCount
			};
			App.Service.DatasetRecommender.Model.MatheRecommendationResponse response = await this._datasetRecommenderService.RecommendMatheAsync(serviceRequest);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.DatasetRecommender.AsAccountable());

			Guid? conversationId = await this.UpdateConversation(lookup.ConversationOptions?.ConversationId, lookup.ConversationOptions?.AutoCreateConversation, lookup.Question, null, new MatheRecommendationQueryConversationEntry
			{
				Version = App.Service.DatasetRecommender.Model.MatheRecommendationRequest.ModelVersion,
				Payload = serviceRequest
			},
			new MatheRecommendationResponseConversationEntry()
			{
				Version = App.Service.DatasetRecommender.Model.MatheRecommendationResponse.ModelVersion,
				Payload = response
			});

			return new SearchResult<App.Service.DatasetRecommender.Model.MatheRecommendationResponse>(conversationId, response);
		}

		[HttpPost("language/linguistic-features")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(LanguagePilotLookup.RequestValidator), "lookup")]
		[SwaggerOperation(Summary = "Discover linguistic features")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<LanguagePilotResponse>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<LanguagePilotResponse>> LinguisticFeaturesAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The field set to apply for building the results", Required = true)]
			LanguagePilotLookup lookup
			)
		{
			this._logger.Debug(new MapLogEntry("linguistic features").And("request", lookup));

			Boolean canExecute = await this._authorizationContentResolver.HasPermission(Permission.CanExecuteLinguisticFeatures);
			if (!canExecute) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			var corpusAnalysisRequest = new LanguagePilotRequest
			{
				DatasetIds = lookup.DatasetIds,
				IncludedFeatures = lookup.IncludedFeatures,
				Query = lookup.Query
			};
			CorpusAnalysisResponse crossDatasetDiscoveryResponse = await this._crossDatasetDiscoveryService.CorpusAnalysisAsync(corpusAnalysisRequest);

			LanguagePilotResponse inDatasetDiscoveryResponse = await this._inDatasetDiscoveryService.LinguisticFeaturesAsync(new LinguisticFeaturesRequest
			{
				Question = lookup.Query,
				RagOutput = crossDatasetDiscoveryResponse,
				RequestedFeatures = this._inDatasetDiscoveryService.MapLinguisticFeatureFlag(lookup.IncludedFeatures) ?? []
			});
			inDatasetDiscoveryResponse.RagOutput = crossDatasetDiscoveryResponse;

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.CrossDatasetDiscovery.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.InDataExploration.AsAccountable());

			Guid? conversationId = await this.UpdateConversation(lookup.ConversationOptions?.ConversationId, lookup.ConversationOptions?.AutoCreateConversation, lookup.Query, null,
			new LinguisticFeaturesQueryConversationEntry()
			{
				Version = LanguagePilotRequest.ModelVersion,
				Payload = corpusAnalysisRequest
			},
			new LinguisticFeaturesResponseConversationEntry()
			{
				Version = CorpusAnalysisResponse.ModelVersion,
				Payload = inDatasetDiscoveryResponse
			});

			return new SearchResult<LanguagePilotResponse>(
				conversationId,
				inDatasetDiscoveryResponse);
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
