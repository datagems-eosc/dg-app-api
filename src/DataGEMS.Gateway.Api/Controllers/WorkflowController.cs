using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
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

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/workflow")]
	public class WorkflowController : ControllerBase
	{
		private readonly ILogger<WorkflowController> _logger;
		private readonly CensorFactory _censorFactory;
		private readonly ErrorThesaurus _errors;
		private readonly BuilderFactory _builderFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IAccountingService _accountingService;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public WorkflowController(
			ILogger<WorkflowController> logger,
			CensorFactory censorFactory,
			IAccountingService accountingService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ErrorThesaurus errors)
		{
			this._logger = logger;
			this._errors = errors;
			this._accountingService = accountingService;
			this._localizer = localizer;
			this._censorFactory = censorFactory;
		}

		[SwaggerOperation(Summary = "Retrieve the available workflow definitions")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching workflows along with the count", type: typeof(QueryResult<App.Model.WorkflowDefinition>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		[HttpGet("definition/query")]
		public async Task<QueryResult<App.Model.WorkflowDefinition>> WorkflowDefinitionQuery(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			WorkflowDefinitionLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.WorkflowDefinition)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowDefinitionCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowDefinitionHttpQuery query = lookup.Enrich(this._queryFactory);
			List<App.Service.Airflow.Model.AirflowDag> datas = await query.CollectAsync();
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : datas.Count;
			List<App.Model.WorkflowDefinition> models = await this._builderFactory.Builder<WorkflowDefinitionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return new QueryResult<App.Model.WorkflowDefinition>(models, count);
		}

		[HttpGet("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup workflow definition by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching workflow definition", type: typeof(QueryResult<App.Model.WorkflowDefinition>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.WorkflowDefinition> Get(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			String id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.WorkflowDefinition)).And("id", id).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowDefinitionCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowDefinitionHttpQuery query = this._queryFactory.Query<WorkflowDefinitionHttpQuery>().Id(id);
			App.Service.Airflow.Model.AirflowDag data = await query.ByIdAsync();
			App.Model.WorkflowDefinition model = await this._builderFactory.Builder<WorkflowDefinitionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Collection)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return model;
		}
	}
}
