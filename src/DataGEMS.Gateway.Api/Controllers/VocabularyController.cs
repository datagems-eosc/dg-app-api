using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/vocabulary")]
	[ApiController]
	public class VocabularyController : ControllerBase
	{
		private readonly ILogger<ConversationController> _logger;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ErrorThesaurus _errors;
		private readonly IConfiguration _configuration;

		public VocabularyController(
			ILogger<ConversationController> logger,
			IAuthorizationContentResolver authorizationContentResolver,
			ErrorThesaurus errors,
			IConfiguration configuration)
		{
			this._logger = logger;
			this._authorizationContentResolver = authorizationContentResolver;
			this._errors = errors;
			this._configuration = configuration;
		}

		[HttpGet("fields-of-science")]
		[Authorize]
		[SwaggerOperation(Summary = "Returns the fields of science vocabulary")]
		[SwaggerResponse(statusCode: 200, description: "Successfully retrieved the fields of science vocabulary", type: typeof(Vocabulary))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The user does not have permission to access this resource")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Vocabulary> GetFieldsOfScienceVocabulary()
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(Vocabulary)));

			bool hasPermission = await this._authorizationContentResolver.HasPermission(Permission.BrowseFieldsOfScienceVocabulary);
			if (!hasPermission)	throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Vocabulary vocabulary = new Vocabulary();
			this._configuration.Bind(vocabulary);

			return vocabulary;
		}

		public class Vocabulary
		{
			[JsonProperty("FieldsOfScience")]
			public FieldsOfScience FieldsOfScience { get; set; }

			[JsonProperty("HttpCache")]
			public HttpCache HttpCache { get; set; }
		}

		public class FieldsOfScience
		{
			[JsonProperty("Hierarchy")]
			public List<HierarchyItem> Hierarchy { get; set; }
		}

		public class HierarchyItem
		{
			[JsonProperty("Ordinal")]
			public int Ordinal { get; set; }

			[JsonProperty("Code")]
			public string Code { get; set; }

			[JsonProperty("Name")]
			public string Name { get; set; }

			[JsonProperty("Children")]
			public List<ChildItem> Children { get; set; }
		}

		public class ChildItem
		{
			[JsonProperty("Ordinal")]
			public int Ordinal { get; set; }

			[JsonProperty("Code")]
			public string Code { get; set; }

			[JsonProperty("Name")]
			public string Name { get; set; }
		}

		public class HttpCache
		{
			[JsonProperty("FieldsOfScienceCacheSeconds")]
			public int FieldsOfScienceCacheSeconds { get; set; }
		}
	}
}
