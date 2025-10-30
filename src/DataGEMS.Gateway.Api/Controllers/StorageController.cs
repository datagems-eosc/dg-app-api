using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.OpenApi;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model.Builder;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

		public StorageController(
			ILogger<StorageController> logger,
			IStorageService storageService,
			IAccountingService accountingService,
			ErrorThesaurus errors,
			App.Authorization.IAuthorizationService authorizationService)
		{
			this._logger = logger;
			this._storageService = storageService;
			this._accountingService = accountingService;
			this._errors = errors;
			this._authorizationService = authorizationService;
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

	}
}
