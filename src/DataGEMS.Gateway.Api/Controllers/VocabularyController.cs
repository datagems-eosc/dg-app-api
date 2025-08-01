using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Service.Vocabulary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/vocabulary")]
	[ApiController]
	public class VocabularyController : ControllerBase
	{
		private readonly ILogger<ConversationController> _logger;
		private readonly App.Authorization.IAuthorizationService _authorizationService;
		private readonly ErrorThesaurus _errors;
		private readonly IAccountingService _accountingService;
		private readonly App.Service.Vocabulary.FieldsOfScienceVocabulary _fieldsOfScienceVocabulary;
		private readonly IConfiguration _configuration;
		private readonly App.Service.Vocabulary.LicenseVocabulary _licenseVocabulary;

		public VocabularyController(
			ILogger<ConversationController> logger,
			App.Authorization.IAuthorizationService authorizationService,
			App.Service.Vocabulary.FieldsOfScienceVocabulary fieldsOfScienceVocabulary,
			IAccountingService accountingService,
			ErrorThesaurus errors,
			IConfiguration configuration,
			App.Service.Vocabulary.LicenseVocabulary licenseVocabulary)
		{
			this._logger = logger;
			this._authorizationService = authorizationService;
			this._fieldsOfScienceVocabulary = fieldsOfScienceVocabulary;
			this._accountingService = accountingService;
			this._errors = errors;
			this._configuration = configuration;
			this._licenseVocabulary = licenseVocabulary;
		}

		[HttpGet("fields-of-science")]
		[Authorize]
		[SwaggerOperation(Summary = "Returns the fields of science vocabulary")]
		[SwaggerResponse(statusCode: 200, description: "Successfully retrieved the fields of science vocabulary", type: typeof(App.Service.Vocabulary.FieldsOfScienceVocabulary))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The user does not have permission to access this resource")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		[ResponseCache(VaryByHeader = "User-Agent", Duration = 3600, Location = ResponseCacheLocation.Client)]
		public async Task<App.Service.Vocabulary.FieldsOfScienceVocabulary> GetFieldsOfScienceVocabulary()
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Service.Vocabulary.FieldsOfScienceVocabulary)));

			await this._authorizationService.AuthorizeForce(Permission.BrowseFieldsOfScienceVocabulary);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Vocabulary.AsAccountable());

			return this._fieldsOfScienceVocabulary;
		}

		[HttpGet("license")]
		[Authorize]
		[SwaggerOperation(Summary = " Returns the license vocabulary")]
		[SwaggerResponse(statusCode: 200, description: "Successfully retrieved the license vocabulary", type: typeof(App.Service.Vocabulary.LicenseVocabulary))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The user does not have permission to access this resource")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		[ResponseCache(VaryByHeader = " User-Agent", Duration = 3600, Location = ResponseCacheLocation.Client)]
		public async Task<App.Service.Vocabulary.LicenseVocabulary> GetLicenceVocabulary([FromQuery(Name = "like")] string like)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Service.Vocabulary.LicenseVocabulary)));

			await this._authorizationService.AuthorizeForce(Permission.BrowseLicenseVocabulary);

			App.Service.Vocabulary.LicenseVocabulary model = this._licenseVocabulary;
			if (!String.IsNullOrEmpty(like))
			{
				List<LicenseVocabulary.License> filtered = this._licenseVocabulary?.Licenses?.Where(x =>
					(x.Name?.Contains(like, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(x.Code?.Contains(like, StringComparison.OrdinalIgnoreCase) ?? false))?.ToList();
				model = new LicenseVocabulary() { Licenses = filtered };
			}

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Vocabulary.AsAccountable());

			return model;
		}
	}
}
