using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.OpenApi;
using DataGEMS.Gateway.Api.Transaction;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
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


		[HttpPost("onboard")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.DatasetPersist.OnboardValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Onboard dataset")]
		[SwaggerResponse(statusCode: 200, description: "The onboarded dataset id", type: typeof(WorkflowProcess))]
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
			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(fieldSet, CensorContext.AsCensor(), !model.Id.HasValue);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			WorkflowProcess process = await this._workflowProcessService.ExecuteOnboardingFlow(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Onboard, KnownResources.Dataset.AsAccountable());
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
