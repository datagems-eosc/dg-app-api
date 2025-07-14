﻿using Cite.Tools.Data.Censor;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Conversation;
using DataGEMS.Gateway.App.Service.Discovery;
using DataGEMS.Gateway.App.Service.InDataExploration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/search")]
	[ApiController]
	public class SearchController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly ICrossDatasetDiscoveryService _crossDatasetDiscoveryService;
		private readonly IInDataExplorationService _inDataExplorationService;
		private readonly ILogger<SearchController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;
		private readonly IConversationService _conversationService;

		public SearchController(
			CensorFactory censorFactory,
			ICrossDatasetDiscoveryService crossDatasetDiscoveryService,
			IInDataExplorationService inDataExplorationService,
			IAccountingService accountingService,
			ILogger<SearchController> logger,
			IConversationService conversationService,
			ErrorThesaurus errors)
		{
			this._censorFactory = censorFactory;
			this._crossDatasetDiscoveryService = crossDatasetDiscoveryService;
			this._inDataExplorationService = inDataExplorationService;
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
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.CrossDatasetDiscovery>>))]
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
				lookup.ConversationOptions?.ConversationId,
				lookup.ConversationOptions?.AutoCreateConversation,
				null,
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

		[HttpPost("in-data/geo-query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(InDataExplorationGeoLookup.InDataExplorationGeoLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "geospatial-query search")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.InDataGeoQueryExploration>>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<List<App.Model.InDataGeoQueryExploration>>> GeospatialQueryAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The exploration query", Required = true)]
			InDataExplorationGeoLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("geospatial query exploring").And("type", nameof(App.Model.InDataGeoQueryExploration)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<InDataExplorationGeoCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ExploreGeoQueryInfo request = new ExploreGeoQueryInfo()
			{
				Question = lookup.Query
			};

			List<App.Model.InDataGeoQueryExploration> results = await this._inDataExplorationService.ExploreGeoQueryAsync(request, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.InDataExplorationGeoQuery.AsAccountable());

			Guid? conversationId = await this.UpdateConversation(
				lookup.ConversationOptions?.ConversationId,
				lookup.ConversationOptions?.AutoCreateConversation,
				null,   //TODO: query's list of datasets????
				new App.Common.Conversation.InDataGeoQueryConversationEntry()
				{
					Version = ExploreGeoQueryInfo.ModelVersion,
					Payload = request
				},
				new App.Common.Conversation.InDataGeoResponseConversationEntry()
				{
					Version = App.Model.InDataGeoQueryExploration.ModelVersion,
					Payload = results
				});

			return new SearchResult<List<App.Model.InDataGeoQueryExploration>>(conversationId, results);
		}


		[HttpPost("in-data/text-to-sql")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(InDataExplorationSqlLookup.InDataExplorationTextToSqlLookupValidator), "lookup")]
		[SwaggerOperation(Summary = "Text to SQL exploration")]
		[SwaggerResponse(statusCode: 200, description: "Matching results", type: typeof(SearchResult<List<App.Model.InDataTextToSqlExploration>>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<SearchResult<List<App.Model.InDataTextToSqlExploration>>> TextToSqlAsync(
			[FromBody]
			[SwaggerRequestBody(description: "The exploration query", Required = true)]
			InDataExplorationSqlLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("text-to-sql exploring").And("type", nameof(App.Model.InDataTextToSqlExploration)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<InDataExplorationSqlCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ExploreTextToSqlInfo request = new ExploreTextToSqlInfo()
			{
				Question = lookup.Query,
				Parameters = lookup.Parameters
			};

			List<App.Model.InDataTextToSqlExploration> results = await this._inDataExplorationService.ExploreTextToSqlAsync(request, censoredFields);

			this._accountingService.AccountFor(KnownActions.Invoke, KnownResources.InDataExplorationTextToSql.AsAccountable());



			Guid? conversationId = await this.UpdateConversation(
				lookup.ConversationOptions?.ConversationId,
				lookup.ConversationOptions?.AutoCreateConversation,
				null,
				new App.Common.Conversation.InDataTextToSqlQueryConversationEntry()
				{
					Version = ExploreTextToSqlInfo.ModelVersion,
					Payload = request
				},
				new App.Common.Conversation.InDataTextToSqlResponseConversationEntry()
				{
					Version = App.Model.InDataTextToSqlExploration.ModelVersion,
					Payload = results
				});

			return new SearchResult<List<App.Model.InDataTextToSqlExploration>>(conversationId, results);
		}


		private async Task<Guid?> UpdateConversation(Guid? conversationId, Boolean? autoCreateConversation, IEnumerable<Guid> datasetIds, params App.Common.Conversation.ConversationEntry[] entries)
		{
			if (!conversationId.HasValue && (!autoCreateConversation.HasValue || (autoCreateConversation.HasValue && !autoCreateConversation.Value))) return null;

			if (!conversationId.HasValue)
			{
				Conversation model = await this._conversationService.PersistAsync(new ConversationPersist() { Name = DateTime.UtcNow.ToString("yyyy-MM-dd h:mm tt") }, new FieldSet(nameof(Conversation.Id)));
				if (model.Id.HasValue) conversationId = model.Id.Value;
			}
			if (!conversationId.HasValue) return null;

			await this._conversationService.AppendToConversation(conversationId.Value, entries);
			await this._conversationService.SetConversationDatasets(conversationId.Value, datasetIds);
			return conversationId.Value;
		}
	}
}
