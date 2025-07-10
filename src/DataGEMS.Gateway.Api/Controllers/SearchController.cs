using DataGEMS.Gateway.App.Model;
using Microsoft.AspNetCore.Mvc;
using DataGEMS.Gateway.App.Service.Discovery;
using Swashbuckle.AspNetCore.Annotations;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Cite.WebTools.Validation;
using Cite.Tools.Logging;
using Cite.Tools.Data.Censor;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Accounting;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Service.Conversation;
using Cite.Tools.Json;
using Microsoft.OpenApi.Services;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/search")]
	[ApiController]
	public class SearchController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly ICrossDatasetDiscoveryService _crossDatasetDiscoveryService;
		private readonly ILogger<SearchController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;
		private readonly IConversationService _conversationService;

		public SearchController(
			CensorFactory censorFactory,
			ICrossDatasetDiscoveryService crossDatasetDiscoveryService,
			IAccountingService accountingService,
			ILogger<SearchController> logger,
			IConversationService conversationService,
			ErrorThesaurus errors)
		{
			this._censorFactory = censorFactory;
			this._crossDatasetDiscoveryService = crossDatasetDiscoveryService;
			this._accountingService = accountingService;
			this._conversationService = conversationService;
			this._logger = logger;
			this._errors = errors;
		}

		[HttpPost("cross-dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(CrossDatasetDiscoveryLookup.CrossDatasetDiscoveryLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Cross-dataset search")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(List<App.Model.CrossDatasetDiscovery>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<List<App.Model.CrossDatasetDiscovery>>> CrossDatasetDiscoveryAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The discovery query", Required = true)]
			CrossDatasetDiscoveryLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("cross dataset discovering").And("type", nameof(App.Model.CrossDatasetDiscovery)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<CrossDatasetDiscoveryCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DiscoverInfo request = new DiscoverInfo()
			{
				Query = lookup.Query,
				ResultCount = lookup.ResultCount
			};

			List<CrossDatasetDiscovery> results = await this._crossDatasetDiscoveryService.DiscoverAsync(request, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.CrossDatasetDiscovery.AsAccountable());

			Guid? conversationId = await this.UpdateConversation(
				lookup.ConversationId,
				lookup.AutoCreateConversation,
				new App.Common.Conversation.CrossDatasetQueryConversationEntry()
				{
					Version = DiscoverInfo.ModelVersion,
					Payload = request
				},
				new App.Common.Conversation.CrossDatasetResponseConversationEntry()
				{
					Version = CrossDatasetDiscovery.ModelVersion,
					Payload = results
				});

			return new SearchResult<List<CrossDatasetDiscovery>>(conversationId, results);
		}

		private async Task<Guid?> UpdateConversation(Guid? conversationId, Boolean? autoCreateConversation, params App.Common.Conversation.ConversationEntry[] entries)
		{
			if (!conversationId.HasValue && (!autoCreateConversation.HasValue || (autoCreateConversation.HasValue && !autoCreateConversation.Value))) return null;

			if (!conversationId.HasValue)
			{
				Conversation model = await this._conversationService.PersistAsync(new ConversationPersist() { Name = DateTime.UtcNow.ToString("yyyy-MM-dd h:mm tt") }, new FieldSet(nameof(Conversation.Id)));
				if (model.Id.HasValue) conversationId = model.Id.Value;
			}
			if (!conversationId.HasValue) return null;

			await this._conversationService.AppendToConversation(conversationId.Value, entries);
			return conversationId.Value;
		}
	}
}
