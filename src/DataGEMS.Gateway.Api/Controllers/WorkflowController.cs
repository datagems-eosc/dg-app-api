using System.Linq;
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
using DataGEMS.Gateway.App.Service.Airflow;
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
		private readonly IAirflowService _airflowService;

		public WorkflowController(
			ILogger<WorkflowController> logger,
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAccountingService accountingService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ErrorThesaurus errors,
			IAirflowService airflowService
			)
		{
			this._logger = logger;
			this._errors = errors;
			this._accountingService = accountingService;
			this._localizer = localizer;
			this._queryFactory = queryFactory;
			this._censorFactory = censorFactory;
			this._builderFactory = builderFactory;
			this._airflowService = airflowService;
		}

		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the available workflow definitions")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching workflows along with the count", type: typeof(QueryResult<App.Model.WorkflowDefinition>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		[HttpPost("definition/query")]
		public async Task<QueryResult<App.Model.WorkflowDefinition>> WorkflowDefinitionQuery(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			WorkflowDefinitionLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("query").And("type", nameof(App.Model.WorkflowDefinition)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowDefinitionCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowDefinitionHttpQuery query = lookup.Enrich(this._queryFactory);
			List<App.Service.Airflow.Model.AirflowDag> datas = await query.CollectAsync();
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : datas.Count;
			List<App.Model.WorkflowDefinition> models = await this._builderFactory.Builder<WorkflowDefinitionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return new QueryResult<App.Model.WorkflowDefinition>(models, count);
		}

		[HttpGet("definition/{id}")]
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
		public async Task<App.Model.WorkflowDefinition> GetWorkflowDefinition(
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
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.WorkflowDefinition)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return model;
		}

		[HttpPost("execute")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(WorkflowExecutionArgs.WorkflowExecutionArgsValidator), "model")]
		[SwaggerOperation(Summary = "Lookup workflow definition by id")]
		[SwaggerResponse(statusCode: 200, description: "Trigger a workflow run", type: typeof(QueryResult<App.Model.WorkflowDefinition>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.WorkflowExecution> Post(
			[FromBody]
			[SwaggerParameter(description: "The model describing the execution parameters", Required = true)]
			WorkflowExecutionArgs model,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("execute").And("type", nameof(App.Model.WorkflowExecution)).And("model", model).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowExecutionCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowExecution execution = await this._airflowService.ExecuteWorkflowAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.Workflow.AsAccountable());

			return execution;
		}

		[HttpGet("definition/{workflowId}/execution/{executionId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup workflow exection by id of definition by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching workflow execution", type: typeof(QueryResult<App.Model.WorkflowExecution>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.WorkflowExecution> GetWorkflowExecution(
			[FromRoute]
			[SwaggerParameter(description: "The workflow id of the item to lookup", Required = true)]
			String workflowId,
			[FromRoute]
			[SwaggerParameter(description: "The execution id of the item to lookup", Required = true)]
			String executionId,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.WorkflowDefinition)).And("workflowId", workflowId).And("executionId", executionId).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowExecutionCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowExecutionHttpQuery query = this._queryFactory.Query<WorkflowExecutionHttpQuery>().Id(executionId).WorkflowIds(workflowId);
			App.Service.Airflow.Model.AirflowDagExecution data = await query.ByIdAsync();
			App.Model.WorkflowExecution model = await this._builderFactory.Builder<WorkflowExecutionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", executionId, nameof(App.Model.WorkflowExecution)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return model;
		}

		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the available workflow executions")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching workflow executions along with the count", type: typeof(QueryResult<App.Model.WorkflowExecution>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[HttpPost("execution/query")]
		public async Task<QueryResult<App.Model.WorkflowExecution>> WorkflowExecutionQuery(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			WorkflowExecutionLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("query").And("type", nameof(App.Model.WorkflowExecution)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowExecutionCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowExecutionHttpQuery query = lookup.Enrich(this._queryFactory);
			List<App.Service.Airflow.Model.AirflowDagExecution> datas = await query.CollectAsync();
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : datas.Count;
			List<App.Model.WorkflowExecution> models = await this._builderFactory.Builder<WorkflowExecutionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return new QueryResult<App.Model.WorkflowExecution>(models, count);
		}

		[HttpGet("definition/{workflowId}/execution/{executionId}/task/{taskId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup workflow task instance by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching workflow task instance", type: typeof(QueryResult<App.Model.WorkflowTaskInstance>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.WorkflowTaskInstance> GetWorkflowTaskInstance(
			[FromRoute]
			[SwaggerParameter(description: "The workflow id of the item to lookup", Required = true)]
			String workflowId,
			[FromRoute]
			[SwaggerParameter(description: "The execution id of the item to lookup", Required = true)]
			String executionId,
			[FromRoute]
			[SwaggerParameter(description: "The task id of the item to lookup", Required = true)]
			String taskId,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.WorkflowTaskInstance)).And("workflowId", workflowId).And("executionId", executionId).And("taskId", taskId).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowTaskInstanceCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowTaskInstanceHttpQuery query = this._queryFactory.Query<WorkflowTaskInstanceHttpQuery>().WorkflowIds(workflowId).WorkflowExecutionIds(executionId).TaskIds(taskId);
			App.Service.Airflow.Model.AirflowTaskInstance data = await query.ByIdAsync();
			App.Model.WorkflowTaskInstance model = await this._builderFactory.Builder<WorkflowTaskInstanceBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", taskId, nameof(App.Model.WorkflowTaskInstance)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return model;
		}

		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the available workflow task instances")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching workflow task instances along with the count", type: typeof(QueryResult<App.Model.WorkflowTaskInstance>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[HttpPost("task/instance/query")]
		public async Task<QueryResult<App.Model.WorkflowTaskInstance>> WorkflowTaskInstanceQuery(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			WorkflowTaskInstanceLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("query").And("type", nameof(App.Model.WorkflowTaskInstance)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowTaskInstanceCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowTaskInstanceHttpQuery query = lookup.Enrich(this._queryFactory);
			List<App.Service.Airflow.Model.AirflowTaskInstance> datas = await query.CollectAsync();
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : datas.Count;
			List<App.Model.WorkflowTaskInstance> models = await this._builderFactory.Builder<WorkflowTaskInstanceBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return new QueryResult<App.Model.WorkflowTaskInstance>(models, count);
		}


		[HttpGet("definition/{workflowId}/execution/{workflowExecutionId}/taskinstance/{workflowTaskId}/logs/{tryNumber}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = " The workflow log by taskid dagid and dagrunid and tryNumber")]
		[SwaggerResponse(statusCode: 200, description: "The matching workflow task log", type: typeof(QueryResult<App.Model.WorkflowTaskLog>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.WorkflowTaskLog> GetWorkflowTaskLog(
			[FromRoute]
			[SwaggerParameter(description: "The workflow id of the item to lookup", Required = true)]
			String workflowExecutionId,
			[FromRoute]
			[SwaggerParameter(description: "The workflow execution id of the item to lookup", Required = true)]
			String workflowId,
			[FromRoute]
			[SwaggerParameter(description: "The workflow task id of the item to lookup", Required = true)]
			String workflowTaskId,
			[FromRoute]
			[SwaggerParameter(description: "The execution try number of the item to lookup", Required = true)]
			int tryNumber,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.WorkflowTaskLog)).And("tryNumber", tryNumber).And("taskId", workflowTaskId).And("dagId", workflowId).And("dagRunId", workflowExecutionId).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowTaskLogsCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowTaskLogsHttpQuery query = this._queryFactory.Query<WorkflowTaskLogsHttpQuery>().TryNumber(tryNumber).WorkflowTaskId(workflowTaskId).WorkflowId(workflowId).WorkflowExecutionId(workflowExecutionId);
			App.Service.Airflow.Model.AirflowTaskLog datas = await query.ByIdAsync();
			App.Model.WorkflowTaskLog model = await this._builderFactory.Builder<WorkflowTaskLogBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", workflowTaskId, nameof(App.Model.WorkflowTaskLog)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Workflow.AsAccountable());

			return model;
		}

	}
}
