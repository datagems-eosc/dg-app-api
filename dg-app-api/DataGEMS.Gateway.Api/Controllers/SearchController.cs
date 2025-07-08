using DataGEMS.Gateway.App.Model;
using Microsoft.AspNetCore.Mvc;
using DataGEMS.Gateway.App.Service.Discovery;
using Swashbuckle.AspNetCore.Annotations;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Cite.WebTools.Validation;
using Cite.Tools.Logging;
using Cite.Tools.Data.Censor;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Accounting;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/search")]
	[ApiController]
	public class SearchController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly ICrossDatasetDiscoveryService _crossDatasetDiscoveryService;
		private readonly ILogger<SearchController> _logger;
		private readonly IAccountingService _accountingService;

		public SearchController(
			ICrossDatasetDiscoveryService crossDatasetDiscoveryService,
			IAccountingService accountingService,
			ILogger<SearchController> logger)
		{
			this._crossDatasetDiscoveryService = crossDatasetDiscoveryService;
			this._accountingService = accountingService;
			this._logger = logger;
		}

		[HttpPost("cross-dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(CrossDatasetDiscoveryLookup.CrossDatasetDiscoveryLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Cross-dataset search")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(List<App.Model.CrossDatasetDiscovery>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Model.CrossDatasetDiscovery>> CrossDatasetDiscoveryAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The discovery query", Required = true)]
			CrossDatasetDiscoveryLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("cross dataset discovering").And("type", nameof(App.Model.CrossDatasetDiscovery)).And("lookup", lookup));

			DiscoverInfo request = new DiscoverInfo()
			{
				Query = lookup.Query,
				ResultCount = lookup.ResultCount
			};

			List<CrossDatasetDiscovery> results = await this._crossDatasetDiscoveryService.DiscoverAsync(request, lookup.Project);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.CrossDatasetDiscovery.AsAccountable());

			return results;
		}
	}
}
