using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
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
using DataGEMS.Gateway.App.Service.DatasetLinking;
using DataGEMS.Gateway.App.Service.TaskOrchestrator;
using DataGEMS.Gateway.App.Service.WorkflowProcess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/dataset")]
	public class DatasetController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<DatasetController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
		private readonly IWorkflowProcessService _workflowProcessService;
		private readonly ITaskOrchestratorService _taskOrchestratorService;
		private readonly IDatasetLinkingService _datasetLinkingService;
		private readonly JsonHandlingService _jsonHandlingService;

		public DatasetController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<DatasetController> logger,
			IAccountingService accountingService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ErrorThesaurus errors,
			IWorkflowProcessService workflowProcessService,
			ITaskOrchestratorService taskOrchestratorService,
			IDatasetLinkingService datasetLinkingService,
			JsonHandlingService jsonHandlingService)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._localizer = localizer;
			this._errors = errors;
			this._workflowProcessService = workflowProcessService;
			this._taskOrchestratorService = taskOrchestratorService;
			this._datasetLinkingService = datasetLinkingService;
			this._jsonHandlingService = jsonHandlingService;
		}

		[HttpPost("query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(DatasetLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query datasets")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching datasets along with the count", type: typeof(QueryResult<App.Model.Dataset>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.Dataset>> Query(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates. The maximum number of results to return is 100.", Required = true)] 
			DatasetLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.Dataset)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DatasetHttpQuery query = lookup.Enrich(this._queryFactory);
			DatasetHttpQuery.QueryResult queryResult = await query.CollectAsync();
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? queryResult.Total : queryResult.Count;
			List<App.Model.Dataset> models = await this._builderFactory.Builder<DatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, queryResult.Items);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Dataset.AsAccountable());

			return new QueryResult<App.Model.Dataset>(models, count);
		}

		[HttpGet("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup dataset by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching dataset", type: typeof(QueryResult<App.Model.Dataset>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.Dataset> Get(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)] 
			Guid id, 
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.Dataset)).And("id", id).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(fieldSet, CensorContext.AsCensor(), datasetId: id);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DatasetHttpQuery query = this._queryFactory.Query<DatasetHttpQuery>().Ids(id);
			App.Service.DataManagement.Model.Dataset data = ((await query.CollectAsync()))?.Items.FirstOrDefault();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);
			App.Model.Dataset model = await this._builderFactory.Builder<DatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Dataset.AsAccountable());

			return model;
		}

		private const String GetPropertiesDescription = """
			Returns the properties of a dataset, deconstructed from the dataset profile (the Analytical Pattern graph produced by the dataset profiler) into a typed hierarchy:

			```
			dataset            the dataset metadata (sc:Dataset)
			distributions[]    files, sheets, file sets, database tables and connections (cr:FileObject, cr:FileSet, dg:DatabaseConnection)
			recordSets[]       logical tables / sheets (cr:RecordSet)
			  fields[]         the columns of the record set (cr:Field)
			  dataQuality[]    data quality assessments (dg:DataQuality)
			    errors[]       individual data quality issues (dg:DataQualityError)
			```

			Column statistics that may be present in the profile are **not** exposed.

			### Selecting properties

			Only the properties requested through the `f` query parameter are returned; everything else is omitted from the response. Nested properties are requested with `.` qualified names, repeating the parameter once per property. Property names are case-insensitive.

			| Group | Available properties |
			|---|---|
			| `dataset` | `id`, `name`, `description`, `headline`, `license`, `url`, `citeAs`, `doi`, `version`, `conformsTo`, `access`, `status`, `archivedAt`, `uploadedBy`, `datePublished`, `keywords`, `fieldOfScience`, `language`, `country` |
			| `distributions` | `id`, `kind`, `labels`, `type`, `name`, `description`, `contentUrl`, `contentSize`, `encodingFormat`, `sha256`, `includes`, `containedIn` |
			| `recordSets` | `id`, `name`, `examples` |
			| `recordSets.fields` | `id`, `name`, `column`, `description`, `dataType`, `semanticType`, `sample`, `sources` |
			| `recordSets.dataQuality` | `id`, `summary` |
			| `recordSets.dataQuality.errors` | `id`, `errorType`, `column`, `description`, `totalAffectedRows`, `examples` |

			Examples:

			* Dataset overview: `f=dataset.name&f=dataset.description&f=dataset.keywords&f=dataset.license`
			* Files of the dataset: `f=distributions.id&f=distributions.kind&f=distributions.name&f=distributions.encodingFormat&f=distributions.containedIn`
			* Schema of the dataset: `f=recordSets.name&f=recordSets.fields.name&f=recordSets.fields.dataType&f=recordSets.fields.semanticType`
			* Data quality report: `f=recordSets.name&f=recordSets.dataQuality.summary&f=recordSets.dataQuality.errors.column&f=recordSets.dataQuality.errors.errorType&f=recordSets.dataQuality.errors.description&f=recordSets.dataQuality.errors.totalAffectedRows`

			### Notes

			* Relations between distributions and fields are expressed through ids: `distributions.containedIn` holds the ids of the containing distributions (a sheet in its workbook, a table in its database connection) and `recordSets.fields.sources` the ids of the distributions a field is read from. Request the respective `id` properties to resolve them.
			* `distributions.id` of file distributions can be used as the `fileObjectNodeId` of the file download endpoint (`GET api/storage/download/dataset/{datasetId}/file-object/{fileObjectNodeId}`).
			* `distributions.contentUrl` of database connections contains the connection string of the database.
			* Values that are not present in the profile are omitted from the response, even when requested.

			### Authorization

			The same rules as the dataset lookup (`GET api/dataset/{id}`) apply: the caller must hold the *BrowseDataset* permission, either globally or through a context grant on this dataset. Otherwise the request fails with 403.
			""";

		[HttpGet("{id}/properties")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup the properties of a dataset, deconstructed from its profile", Description = GetPropertiesDescription, OperationId = "GetDatasetProperties")]
		[SwaggerResponse(statusCode: 200, description: "The requested properties of the dataset. Only the properties requested through the f parameter are populated", type: typeof(App.Model.DeconstructedDatasetAnalyticalPattern))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request, e.g. a malformed dataset id")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The caller does not hold the BrowseDataset permission, neither globally nor through a context grant on the dataset")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate a dataset with the provided id")]
		[SwaggerResponse(statusCode: 424, description: "The data management service failed to return the dataset")]
		[SwaggerResponse(statusCode: 500, description: "Internal error, including the dataset having no profile yet (it has not been profiled) or an unreadable profile")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.DeconstructedDatasetAnalyticalPattern> GetProperties(
			[FromRoute]
			[SwaggerParameter(description: "The id of the dataset whose properties to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The properties to include in the response model, repeated once per property. Nested properties are qualified with '.' (e.g. recordSets.fields.name). See the operation description for the full list of available properties", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.DeconstructedDatasetAnalyticalPattern)).And("id", id).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<DeconstructedDatasetAnalyticalPatternCensor>().Censor(fieldSet, CensorContext.AsCensor(), id);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DatasetHttpQuery query = this._queryFactory.Query<DatasetHttpQuery>().Ids(id);
			App.Service.DataManagement.Model.Dataset data = ((await query.CollectAsync()))?.Items.FirstOrDefault();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);
			if (data.ProfileRaw == null) throw new DGApplicationException(this._localizer["dataset_noProfile", id]);

			AnalyticalPattern profile = this._jsonHandlingService.FromJsonSafe<AnalyticalPattern>(this._jsonHandlingService.ToJsonSafe(data.ProfileRaw));
			if (profile == null) throw new DGApplicationException(this._localizer["dataset_noProfile", id]);
			App.Model.DeconstructedDatasetAnalyticalPattern model = await this._builderFactory.Builder<DeconstructedDatasetAnalyticalPatternBuilder>().Build(censoredFields, profile);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Dataset.AsAccountable());

			return model;
		}

		[HttpPost("update")]
		[Authorize]
		[ValidationFilter(typeof(App.Model.DatasetPersist.PersistValidator), "model")]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Update dataset")]
		[SwaggerResponse(statusCode: 200, description: "The updated dataset", type: typeof(QueryResult<App.Model.Dataset>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.Dataset> Update(
			[FromBody]
			[SwaggerParameter(description: "The model to update", Required = true)]
			DatasetPersist model,

			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("update").And("type", nameof(App.Model.Dataset)).And("model", model).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Dataset returnModel = await this._taskOrchestratorService.UpdateDatasetAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.Dataset.AsAccountable());

			return returnModel;
		}

		[HttpPost("linking/refinement")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(App.Model.DatasetLinkingRefinement.LinkingRefinementValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Refine dataset linking")]
		[SwaggerResponse(statusCode: 200, description: "The refined linking result", type: typeof(string))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<string> LinkingRefinement(
			[FromBody]
			[SwaggerRequestBody(description: "The dataset linking refinement to apply", Required = true)]
			App.Model.DatasetLinkingRefinement model)
		{
			this._logger.Debug(new MapLogEntry("linking-refinement").And("model", model));
			string result = await this._datasetLinkingService.RefineLinkingAsync(model);
			this._accountingService.AccountFor(KnownActions.LinkingRefinement, KnownResources.Dataset.AsAccountable());
			return result;
		}

		[HttpPost("linking/status")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Get dataset linking status")]
		[SwaggerResponse(statusCode: 200, description: "The status of the linking job", type: typeof(DatasetLinkingStatus))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
        [Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
        [Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<DatasetLinkingStatus> GetLinkingStatus(
			[FromBody]
			[SwaggerParameter(description: "The id of the job", Required = true)]
			string id)
		{ 
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.Dataset)).And("id", id));

			DatasetLinkingStatus result = await this._datasetLinkingService.GetJobStatusByIdAsync(id);

			this._accountingService.AccountFor(KnownActions.LinkingStatus, KnownResources.Dataset.AsAccountable());

			return result;
		}

		[HttpPost("linking/result")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Get dataset linking result")]
		[SwaggerResponse(statusCode: 200, description: "The matching dataset", type: typeof(string))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
        [Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
        [Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<string> GetLinkingResult(
			[FromBody]
			[SwaggerParameter(description: "The id of the job", Required = true)]
			string id)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.Dataset)).And("id", id));

			string result = await this._datasetLinkingService.GetJobByIdAsync(id);

			this._accountingService.AccountFor(KnownActions.LinkingResult, KnownResources.Dataset.AsAccountable());

			return result;
		}
	}
}
