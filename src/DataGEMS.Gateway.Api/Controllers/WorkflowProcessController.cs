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
using DataGEMS.Gateway.Api.Transaction;
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
using DataGEMS.Gateway.App.Service.DataManagement;
using DataGEMS.Gateway.App.Service.WorkflowProcess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/temp")]
	[ApiController]
	public class WorkflowProcessController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<DatasetController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly IDataManagementService _datasetService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
		private readonly IWorkflowProcessService _workflowProcessService;

		public WorkflowProcessController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<DatasetController> logger,
			IAccountingService accountingService,
			IDataManagementService datasetService,
			ErrorThesaurus errors,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			IWorkflowProcessService workflowProcessService
			)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._datasetService = datasetService;
			this._errors = errors;
			this._localizer = localizer;
			this._workflowProcessService = workflowProcessService;
		}

		[HttpPost("query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(WorkflowProcessLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query workflow processes")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching workflow processes along with the count", type: typeof(QueryResult<App.Model.WorkflowProcess>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.WorkflowProcess>> Query(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			WorkflowProcessLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.WorkflowProcess)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcessQuery query = lookup.Enrich(this._queryFactory);
			List<App.Data.WorkflowProcess> queryResult = await query.CollectAsync();
			int count = queryResult.Count;
			List<App.Model.WorkflowProcess> models = await this._builderFactory.Builder<WorkflowProcessBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, queryResult);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.WorkflowProcess.AsAccountable());

			return new QueryResult<App.Model.WorkflowProcess>(models, count);
		}

		[HttpGet("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup workflow process by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching workflow process", type: typeof(QueryResult<App.Model.WorkflowProcess>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.WorkflowProcess> Get(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.WorkflowProcess)).And("id", id).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcessQuery query = this._queryFactory.Query<WorkflowProcessQuery>().Ids(id);
			App.Data.WorkflowProcess data = await query.FirstAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Data.WorkflowProcess)]);
			App.Model.WorkflowProcess model = await this._builderFactory.Builder<WorkflowProcessBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.WorkflowProcess)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.WorkflowProcess.AsAccountable());

			return model;
		}


		[HttpPost("step/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(WorkflowProcessStepLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query workflow process steps")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching workflow processes along with the count", type: typeof(QueryResult<App.Model.WorkflowProcessStep>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.WorkflowProcessStep>> StepQuery(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			WorkflowProcessStepLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.WorkflowProcessStep)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessStepCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcessStepQuery query = lookup.Enrich(this._queryFactory);
			List<App.Data.WorkflowProcessStep> queryResult = await query.CollectAsync();
			int count = queryResult.Count;
			List<App.Model.WorkflowProcessStep> models = await this._builderFactory.Builder<WorkflowProcessStepBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, queryResult);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.WorkflowProcessStep.AsAccountable());

			return new QueryResult<App.Model.WorkflowProcessStep>(models, count);
		}

		[HttpGet("step/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup workflow process step by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching workflow process step", type: typeof(QueryResult<App.Model.WorkflowProcessStep>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.WorkflowProcessStep> StepGet(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.WorkflowProcessStep)).And("id", id).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessStepCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcessStepQuery query = this._queryFactory.Query<WorkflowProcessStepQuery>().Ids(id);
			App.Data.WorkflowProcessStep data = await query.FirstAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Data.WorkflowProcessStep)]);
			App.Model.WorkflowProcessStep model = await this._builderFactory.Builder<WorkflowProcessStepBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.WorkflowProcessStep)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.WorkflowProcessStep.AsAccountable());

			return model;
		}

		[HttpPost("onboard")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.DatasetPersist.OnboardValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Onboard dataset")]
		[SwaggerResponse(statusCode: 200, description: "The onboard dataset process", type: typeof(WorkflowProcess))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<WorkflowProcess> Onboard(
			[FromBody]
			[SwaggerRequestBody(description: "The model to onboard", Required = true)]
			App.Model.DatasetPersist model,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("onboarding").And("type", nameof(App.Model.DatasetPersist)).And("fields", fieldSet));

			//GOTCHA: Ommiting browse permission check in case of new
			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcess process = await this._workflowProcessService.ExecuteOnboardingFlow(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Onboard, KnownResources.Dataset.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());

			return process;
		}


		[HttpPost("profile")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.DatasetProfiling.ProfilingValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Profile dataset")]
		[SwaggerResponse(statusCode: 200, description: "The profile dataset process", type: typeof(WorkflowProcess))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<WorkflowProcess> Profile(
			[FromBody]
			[SwaggerRequestBody(description: "The profile to apply", Required = true)]
			App.Model.DatasetProfiling model,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("profiling").And("model", model));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcess process = await this._workflowProcessService.ExecuteProfilingFlow(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Profile, KnownResources.Dataset.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());

			return process;
		}


		[HttpPost("package")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.DatasetPackaging.PackagingValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Package dataset")]
		[SwaggerResponse(statusCode: 200, description: "The package process", type: typeof(WorkflowProcess))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<WorkflowProcess> Package(
			[FromBody]
			[SwaggerRequestBody(description: "The package to apply", Required = true)]
			App.Model.DatasetPackaging model,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("packaging").And("model", model));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			var response = await this._workflowProcessService.ExecutePackagingFlow(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Package, KnownResources.Dataset.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());

			return response;
		}


		[HttpPost("recommendation-register")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.DatasetRecommendationRegistering.RecommendationRegisteringValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Register dataset to recommendation")]
		[SwaggerResponse(statusCode: 200, description: "The registered dataset id", type: typeof(WorkflowProcess))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<WorkflowProcess> RecommendationRegister(
			[FromBody]
			[SwaggerRequestBody(description: "The dataset to register to recommendation", Required = true)]
			App.Model.DatasetRecommendationRegistering model,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("recommendation-registering").And("model", model));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcess process = await this._workflowProcessService.ExecuteRecommendationFlow(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.RecommendationRegister, KnownResources.Dataset.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());

			return process;
		}


		[HttpPost("cdd-ingest")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.DatasetCddIngest.CddIngestValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "CDD Ingest dataset")]
		[SwaggerResponse(statusCode: 200, description: "The registered dataset id", type: typeof(WorkflowProcess))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<WorkflowProcess> CddIngest(
			[FromBody]
			[SwaggerRequestBody(description: "The dataset to ingest to CDD", Required = true)]
			App.Model.DatasetCddIngest model,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("cdd-ingest").And("model", model));

			IFieldSet censoredFields = await this._censorFactory.Censor<WorkflowProcessCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcess process = await this._workflowProcessService.ExecuteCddIngestionFlow(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.CddIngest, KnownResources.Dataset.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());

			return process;
		}

		[HttpPost("workflow-process/step/persist")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.WorkflowProcessStepPersist.PersistValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Update a workflow process step")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task WorkflowProcessStepPersist(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			App.Model.WorkflowProcessStepPersist model,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.WorkflowProcessStepPersist)).And("fields", fieldSet));

			await this._workflowProcessService.UpdateWorkflowProcessStep(model);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.WorkflowProcessStep.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());
		}




		[HttpPost("workflow-process/step/finalize-onboarding")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.WorkflowOnboardingStepFinalize.Validator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Update a workflow process step")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task WorkflowOnboardingStepFinalize(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			App.Model.WorkflowOnboardingStepFinalize model)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.WorkflowOnboardingStepFinalize)));

			await this._workflowProcessService.FinilizeOnboardingStep(model.WorkflowProcessStep, model.Profiling);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.WorkflowProcessStep.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());
		}


		[HttpPost("workflow-process/step/finalize-profiling")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.WorkflowProfilingStepFinalize.Validator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Update a workflow process step")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task WorkflowProfilingStepFinalize(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			App.Model.WorkflowProfilingStepFinalize model)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.WorkflowProfilingStepFinalize)));

			await this._workflowProcessService.FinilizeProfilingStep(model.WorkflowProcessStep, model.DatasetId.Value);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.WorkflowProcessStep.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());
		}

		[HttpPost("workflow-process/step/finalize-packaging")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.WorkflowPackagingStepFinalize.Validator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Update a workflow process step")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task WorkflowPackagingStepFinalize(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			App.Model.WorkflowPackagingStepFinalize model)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.WorkflowPackagingStepFinalize)));

			await this._workflowProcessService.FinilizePackagingStep(model.WorkflowProcessStep, model.DatasetId.Value);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.WorkflowProcessStep.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());
		}


		[HttpPost("workflow-process/step/finalize-recommendation")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.WorkflowRecommendationStepFinalize.Validator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Update a workflow process step")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task WorkflowRecommendationStepFinalize(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			App.Model.WorkflowRecommendationStepFinalize model)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.WorkflowRecommendationStepFinalize)));

			await this._workflowProcessService.FinilizeRecommendationStep(model.WorkflowProcessStep, model.DatasetId.Value);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.WorkflowProcessStep.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());
		}

		[HttpPost("workflow-process/step/finalize-cdd-ingestion")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.WorkflowCddIngestionStepFinalize.Validator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Update a workflow process step")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task WorkflowCddIngestionStepFinalize(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			App.Model.WorkflowCddIngestionStepFinalize model)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.WorkflowCddIngestionStepFinalize)));

			await this._workflowProcessService.FinilizeCddIngestionStep(model.WorkflowProcessStep, model.DatasetId.Value);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.WorkflowProcessStep.AsAccountable());
			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.Workflow.AsAccountable());
		}
	}
}
