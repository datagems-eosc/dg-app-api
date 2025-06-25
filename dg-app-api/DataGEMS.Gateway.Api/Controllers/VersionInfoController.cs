using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Service.Version;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/version-info")]
	public class VersionInfoController : ControllerBase
	{
		private readonly IVersionInfoService _versionInfoService;
		private readonly ILogger<VersionInfoController> _logger;

		public VersionInfoController(
			IVersionInfoService versionInfoService,
			ILogger<VersionInfoController> logger)
		{
			this._logger = logger;
			this._versionInfoService = versionInfoService;
		}

		[HttpGet("current")]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve current app version")]
		[SwaggerResponse(statusCode: 200, description: "The current app version", type: typeof(QueryResult<App.Model.VersionInfo>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Model.VersionInfo>> GetCurrent()
		{
			this._logger.Debug(new MapLogEntry("current").And("type", nameof(App.Model.VersionInfo)));

			List<App.Model.VersionInfo> current = await this._versionInfoService.CurrentAsync();
			return current;
		}
	}
}
