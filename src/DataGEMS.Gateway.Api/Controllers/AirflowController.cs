using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
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
using DataGEMS.Gateway.App.Model.Builder;
using DataGEMS.Gateway.App.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;
using DataGEMS.Gateway.App.Service.Airflow;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.Api.Controllers
{
	public class AirflowController : ControllerBase
	{
		private readonly ILogger<AirflowController> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly BuilderFactory _builderFactory;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IAirflowService _airflowService;

		public AirflowController(
			ILogger<AirflowController> logger,
			ErrorThesaurus errors,
			IHttpClientFactory httpClientFactory,
			IAirflowService airflowService)
		{
			this._logger = logger;
			this._errors = errors;
			this._httpClientFactory = httpClientFactory;
			this._airflowService = airflowService;
		}

		[SwaggerOperation(Summary = "Retrieve the workflow for all the available DAGs ")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching user collections along with the count", type: typeof(QueryResult<App.Model.UserCollection>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[HttpGet("dag-runs")]
	
		public async Task<IActionResult> GetDagRuns([FromQuery] FieldSet fields)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.Airflow)).And("fields", fields));

			// request object
			var request = new AirflowInfo();

			List<App.Model.Airflow> models = await this._airflowService.GetDagRunsAsync(request, fields);

			var result = new QueryResult<App.Model.Airflow>
			{
				Count = models.Count,
				Items = models
			};

			return Ok(result);
		}
	}
}

	