using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
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
using DataGEMS.Gateway.App.Service.DatasetFileManagement;
using DataGEMS.Gateway.App.Service.DatasetFileManagement.Model;
using DataGEMS.Gateway.App.Service.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/storage")]
	public class StorageController : ControllerBase
	{
		private readonly ILogger<StorageController> _logger;
		private readonly IStorageService _storageService;
		private readonly IAccountingService _accountingService;
		private readonly App.Authorization.IAuthorizationService _authorizationService;
		private readonly ErrorThesaurus _errors;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IDatasetFileManagementService _datasetFileManagementService;
		private readonly CensorFactory _censorFactory;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
		private readonly BuilderFactory _builderFactory;
		private readonly QueryFactory _queryFactory;

		public StorageController(
			ILogger<StorageController> logger,
			IStorageService storageService,
			IAccountingService accountingService,
			ErrorThesaurus errors,
			App.Authorization.IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
			IDatasetFileManagementService datasetFileManagementService,
			CensorFactory censorFactory,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			BuilderFactory builderFactory,
			QueryFactory queryFactory)
		{
			this._logger = logger;
			this._storageService = storageService;
			this._accountingService = accountingService;
			this._errors = errors;
			this._localizer = localizer;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._datasetFileManagementService = datasetFileManagementService;
			this._censorFactory = censorFactory;
			this._builderFactory = builderFactory;
			this._queryFactory = queryFactory;
		}

		[HttpGet("upload/allowed-extension")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve allowed extensions for upload")]
		[SwaggerResponse(statusCode: 200, description: "The allowed extensions to upload", type: typeof(List<String>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<String>> Get()
		{
			this._logger.Debug(new MapLogEntry("allowed extensions"));

			List<String> allowed = await this._storageService.AllowedExtensions();

			return allowed;
		}

		[HttpPost("upload/dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Upload dataset files")]
		[SwaggerResponse(statusCode: 200, description: "The list of uploaded dataset files", type: typeof(List<String>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<String>> UploadDatasetFiles()
		{
			this._logger.Debug(new MapLogEntry("uploading").And("fileCount", Request?.Form?.Files?.Count));

			await this._authorizationService.AuthorizeForce(Permission.OnboardDataset);

			if (Request?.Form?.Files == null || Request?.Form?.Files?.Count == 0) throw new DGValidationException("No File was provided");

			List<String> allowedExtensions = await this._storageService.AllowedExtensions();
			long allowedSize = await this._storageService.MaxFileUploadSize();

			Boolean foundProblem = Request.Form.Files.Any(x =>
			{
				String extension = System.IO.Path.GetExtension(x.FileName);
				Boolean isAlloweExtension = allowedExtensions.Select(x => x.ToLowerInvariant()).Contains(extension.ToLowerInvariant());

				Boolean isAllowedSize = x.Length <= allowedSize;

				return !isAllowedSize || !isAlloweExtension;
			});
			if (foundProblem) throw new DGValidationException(this._errors.UploadRestricted.Code, this._errors.UploadRestricted.Message);

			List<String> uploaded = new List<string>();
			foreach (IFormFile file in Request.Form.Files)
			{
				String uploadedFile = await this._storageService.PersistAsync(new StorageFile()
				{
					Name = System.IO.Path.GetFileNameWithoutExtension(file.FileName),
					Extension = System.IO.Path.GetExtension(file.FileName),
					MimeType = file.ContentType,
					StorageType = App.Common.StorageType.DatasetFileUpload
				}, file);
				if(!String.IsNullOrEmpty(uploadedFile)) uploaded.Add(uploadedFile);
			}

			this._accountingService.AccountFor(KnownActions.Upload, KnownResources.Dataset.AsAccountable());

			return uploaded;
		}

		[HttpGet("download/dataset/{datasetId}/file-object/{fileObjectNodeId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Download dataset files")]
		[SwaggerResponse(statusCode: 200, description: "The downloaded dataset file", type: typeof(FileContentResult))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		public async Task<FileContentResult> DownloadDatasetFile(
			[FromRoute][SwaggerParameter(description: "The id of the dataset", Required = true)]
			Guid datasetId,
			[FromRoute][SwaggerParameter(description: "The id of the file object node", Required = true)]
			Guid fileObjectNodeId
		)
		{
			this._logger.Debug(new MapLogEntry("downloading").And("dataset id", datasetId).And("file object node id", fileObjectNodeId));

			FileDetails downloadedFile = await this._datasetFileManagementService.DownloadDatasetFileAsync(datasetId, fileObjectNodeId);
			this._accountingService.AccountFor(KnownActions.Download, KnownResources.Dataset.AsAccountable());

			return File(fileContents: downloadedFile.Contents, contentType: downloadedFile.ContentType, fileDownloadName: downloadedFile.FileName);
		}

		[HttpGet("browse/dataset/{datasetId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Browse dataset files")]
		[SwaggerResponse(statusCode: 200, description: "The dataset files", type: typeof(DatasetObject))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<DatasetObject> BrowseDatasetFile(
			[FromRoute][SwaggerParameter(description: "The id of the dataset", Required = true)]
			Guid datasetId,
			[FromQuery][SwaggerParameter(description: "The id of the node", Required = true)]
			Guid? nodeId
		)
		{
			this._logger.Debug(new MapLogEntry("browsing").And("dataset id", datasetId).And("node id", nodeId));

			DatasetObject datasetFileSet = await this._datasetFileManagementService.BrowseDatasetFilesAsync(datasetId, nodeId);

			this._accountingService.AccountFor(KnownActions.BrowseData, KnownResources.Dataset.AsAccountable());

			return datasetFileSet;
		}

		[HttpGet("download/ad-hoc-query/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Download ad-hoc query results")]
		[SwaggerResponse(statusCode: 200, description: "The result", type: typeof(FileContentResult))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		public async Task<FileContentResult> DownloadAdHocResult(
			[FromRoute]
			[SwaggerParameter(description: "The id of the ad-hoc query evaluation item whose output nmust be downloaded", Required = true)]
			Guid id)
		{
			this._logger.Debug(new MapLogEntry("download").And("type", nameof(App.Model.AdHocQuery)).And("id", id));
			
			FileDetails downloadedFile = await this._datasetFileManagementService.DownloadFromAdHocQueryAsync(id);

			this._accountingService.AccountFor(KnownActions.Download, KnownResources.AdHocQuery.AsAccountable());

			Response.Headers["Access-Control-Expose-Headers"] = "Content-Disposition";
			return File(fileContents: downloadedFile.Contents, contentType: downloadedFile.ContentType, fileDownloadName: downloadedFile.FileName);
		}
	}
}
