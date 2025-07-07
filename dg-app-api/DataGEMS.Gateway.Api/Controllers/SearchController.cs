using DataGEMS.Gateway.App.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataGEMS.Gateway.App.Service.Discovery;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.Api.OpenApi;
using Swashbuckle.AspNetCore.Annotations;
using DataGEMS.Gateway.Api.Model;

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

		// TODO: Validation
		[HttpPost("cross-dataset")]
		public async Task<List<App.Model.CrossDatasetDiscovery>> CrossDatasetDiscoveryAsync(
			[FromBody] CrossDatasetDiscoveryLookup lookup,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = false)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{

			// TODO: Converst CrossDatasetDiscoveryLookup lookup to DiscoverInfo request
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
