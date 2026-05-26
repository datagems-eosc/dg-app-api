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
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Model.Builder;
using DataGEMS.Gateway.App.Query;
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
			IInDataExplorationService inDatasetDiscoveryService)
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
		}

		[HttpPost("mathe/recommend")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(MatheRecommendationRequest.RequestValidator), "request")]
		[SwaggerOperation(Summary = "Generate material-based recommendations")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(App.Service.DatasetRecommender.Model.MatheRecommendationResponse))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Service.DatasetRecommender.Model.MatheRecommendationResponse> RecommendDatasetsAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The field set to apply for building the results", Required = true)]
			MatheRecommendationRequest request
			)
		{
			this._logger.Debug(new MapLogEntry("mathE recommendation").And("type", nameof(App.Model.Dataset)).And("request", request));

			App.Service.DatasetRecommender.Model.MatheRecommendationResponse response = await this._datasetRecommenderService.RecommendMatheAsync(new App.Service.DatasetRecommender.Model.MatheRecommendationRequest
			{
				QuestionId = request.QuestionId,
				Question = request.Question,
				RecommendedMaterialsCount = request.RecommendedMaterialsCount
			});

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.DatasetRecommender.AsAccountable());

			return response;
		}

		[HttpPost("language/linguistic-features")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(LanguagePilotRequest.RequestValidator), "request")]
		[SwaggerOperation(Summary = "Discover linguistic features")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(LanguagePilotResponse))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<LanguagePilotResponse> LinguisticFeaturesAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The field set to apply for building the results", Required = true)]
			LanguagePilotRequest request
			)
		{
			this._logger.Debug(new MapLogEntry("linguistic features").And("request", request));

			List<Guid> datasetIds = await this._authorizationContentResolver.EffectiveContextAffiliatedDatasets(Permission.CanExecuteLinguisticFeatures);
			if (datasetIds == null || request.DatasetIds.Any(x => !datasetIds.Contains(x))) throw new DGUnauthorizedException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			CorpusAnalysisResponse crossDatasetDiscoveryResponse = await this._crossDatasetDiscoveryService.CorpusAnalysisAsync(request);

			var inDatasetDiscoveryResponse = await this._inDatasetDiscoveryService.LinguisticFeaturesAsync(new LinguisticFeaturesRequest
			{
				Question = request.Query,
				RagOutput = crossDatasetDiscoveryResponse
			});

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.CrossDatasetDiscovery.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.InDataExploration.AsAccountable());

			return inDatasetDiscoveryResponse;
		}
	}
}
