using DataGEMS.Gateway.App.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataGEMS.Gateway.App.Service.Discovery;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.Api.OpenApi;
using Swashbuckle.AspNetCore.Annotations;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Cite.WebTools.Validation;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/search")]
	[ApiController]
	public class SearchController : ControllerBase
	{
		private readonly ICrossDatasetDiscoveryService _crossDatasetDiscoveryService;

		public SearchController(ICrossDatasetDiscoveryService crossDatasetDiscoveryService)
		{
			_crossDatasetDiscoveryService = crossDatasetDiscoveryService;
		}

		[HttpPost("cross-dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(CrossDatasetDiscoveryLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Cross-dataset search")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(List<App.Model.CrossDatasetDiscovery>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "Forbidden")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "Service unavailable")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Model.CrossDatasetDiscovery>> CrossDatasetDiscoveryAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The discovery query", Required = true)]
			CrossDatasetDiscoveryLookup lookup,
			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "Fields to include in the response", Required = false)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			DiscoverInfo request = new DiscoverInfo()
			{
				Query = lookup.Query,
				ResultCount = lookup.ResultCount
			};

			List<CrossDatasetDiscovery> results = await this._crossDatasetDiscoveryService.DiscoverAsync(request, fieldSet);
			return results;
		}
	}
}
