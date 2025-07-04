using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model.Builder;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.Conversation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.Api.OpenApi;
using DataGEMS.Gateway.Api.Transaction;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/conversation")]
	public class ConversationController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<ConversationController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IConversationService _conversationService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public ConversationController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<ConversationController> logger,
			IAccountingService accountingService,
			IAuthorizationContentResolver authorizationContentResolver,
			IConversationService conversationService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ErrorThesaurus errors)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._conversationService = conversationService;
			this._localizer = localizer;
			this._errors = errors;
		}

		[HttpPost("query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(ConversationLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query conversations")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching conversations along with the count", type: typeof(QueryResult<App.Model.Conversation>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.Conversation>> Query(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			ConversationLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.Conversation)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ConversationQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<App.Data.Conversation> datas = await query.CollectAsync(censoredFields);
			List<App.Model.Conversation> models = await this._builderFactory.Builder<ConversationBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Conversation.AsAccountable());

			return new QueryResult<App.Model.Conversation>(models, count);
		}

		[HttpPost("me/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(ConversationLookup.QueryMeValidator), "lookup")]
		[SwaggerOperation(Summary = "Query user owned conversations")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching user owned conversations along with the count", type: typeof(QueryResult<App.Model.Conversation>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.Conversation>> MeQuery(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			ConversationLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.Conversation)).And("lookup", lookup));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationCensor>().Censor(lookup.Project, CensorContext.AsCensor(), userId);
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ConversationQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any).UserIds(userId.Value);
			List<App.Data.Conversation> datas = await query.CollectAsync(censoredFields);
			List<App.Model.Conversation> models = await this._builderFactory.Builder<ConversationBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Conversation.AsAccountable());

			return new QueryResult<App.Model.Conversation>(models, count);
		}

		[HttpGet("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup conversation by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching conversation", type: typeof(QueryResult<App.Model.Conversation>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.Conversation> Get(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.Conversation)).And("id", id).And("fields", fieldSet));

			Guid userId = await this._queryFactory.Query<ConversationQuery>().Authorize(AuthorizationFlags.None).Ids(id).FirstAsync(x => x.UserId);
			if (userId == default(Guid)) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Conversation)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ConversationQuery query = this._queryFactory.Query<ConversationQuery>().Ids(id).DisableTracking().Authorize(AuthorizationFlags.Any);
			App.Data.Conversation data = await query.FirstAsync(censoredFields);
			App.Model.Conversation model = await this._builderFactory.Builder<ConversationBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Conversation)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Conversation.AsAccountable());

			return model;
		}

		[HttpPost("me/persist")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(ConversationPersist.PersistValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Persist user owned conversation")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user owned conversation", type: typeof(App.Model.Conversation))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Conversation> Persist(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			ConversationPersist model,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.ConversationPersist)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Conversation persisted = await this._conversationService.PersistAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.Conversation.AsAccountable());

			return persisted;
		}

		[HttpPost("me/persist/deep")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(ConversationPersistDeep.PersistValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Persist user owned conversation along with details provided")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user owned conversation", type: typeof(App.Model.Conversation))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Conversation> PersistDeep(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			ConversationPersistDeep model,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.ConversationPersist)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Conversation persisted = await this._conversationService.PersistAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.Conversation.AsAccountable());

			return persisted;
		}

		[HttpPost("me/patch/dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(ConversationDatasetPatch.PatchValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Patch user owned conversation with updated datasets")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user owned conversation", type: typeof(App.Model.Conversation))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Conversation> PatchDatasets(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			ConversationDatasetPatch model,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("patching").And("type", nameof(App.Model.ConversationDatasetPatch)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Conversation persisted = await this._conversationService.PatchAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.Conversation.AsAccountable());

			return persisted;
		}

		[HttpPost("dataset/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(ConversationDatasetLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query conversation datasets")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching conversation datasets along with the count", type: typeof(QueryResult<App.Model.ConversationDataset>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.ConversationDataset>> QueryDataset(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			ConversationDatasetLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.ConversationDataset)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationDatasetCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ConversationDatasetQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<App.Data.ConversationDataset> datas = await query.CollectAsync(censoredFields);
			List<App.Model.ConversationDataset> models = await this._builderFactory.Builder<ConversationDatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Conversation.AsAccountable());

			return new QueryResult<App.Model.ConversationDataset>(models, count);
		}

		[HttpPost("dataset/me/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(ConversationDatasetLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query user owned conversation datasets")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching user owned collection datasets along with the count", type: typeof(QueryResult<App.Model.ConversationDataset>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.ConversationDataset>> MeQueryDataset(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			ConversationDatasetLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.ConversationDataset)).And("lookup", lookup));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationDatasetCensor>().Censor(lookup.Project, CensorContext.AsCensor(), userId);
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ConversationDatasetQuery query = lookup.Enrich(this._queryFactory)
				.DisableTracking()
				.Authorize(AuthorizationFlags.Any)
				.ConversationSubQuery(
					this._queryFactory.Query<ConversationQuery>()
					.Authorize(AuthorizationFlags.Any)
					.UserIds(userId.Value));
			List<App.Data.ConversationDataset> datas = await query.CollectAsync(censoredFields);
			List<App.Model.ConversationDataset> models = await this._builderFactory.Builder<ConversationDatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Conversation.AsAccountable());

			return new QueryResult<App.Model.ConversationDataset>(models, count);
		}

		[HttpGet("dataset/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup conversation dataset by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching conversation dataset", type: typeof(QueryResult<App.Model.ConversationDataset>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.ConversationDataset> GetDataset(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.ConversationDataset)).And("id", id).And("fields", fieldSet));

			Guid userId = await this._queryFactory.Query<ConversationQuery>()
				.Authorize(AuthorizationFlags.None)
				.IsActive(IsActive.Active)
				.ConversationDatasetSubQuery(
					this._queryFactory.Query<ConversationDatasetQuery>()
					.Authorize(AuthorizationFlags.None)
					.IsActive(IsActive.Active)
					.Ids(id))
				.FirstAsync(x => x.UserId);
			if (userId == default(Guid)) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.ConversationDataset)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationDatasetCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			ConversationDatasetQuery query = this._queryFactory.Query<ConversationDatasetQuery>().Ids(id).DisableTracking().Authorize(AuthorizationFlags.Any);
			App.Data.ConversationDataset data = await query.FirstAsync(censoredFields);
			App.Model.ConversationDataset model = await this._builderFactory.Builder<ConversationDatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.ConversationDataset)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Conversation.AsAccountable());

			return model;
		}

		[HttpPost("dataset/me/{conversationId}/{datasetId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Add dataset in owned conversation")]
		[SwaggerResponse(statusCode: 200, description: "The dataset was added and returns the updated conversation", type: typeof(App.Model.Conversation))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.Conversation> AddDatasetInConversation(
			[FromRoute]
			[SwaggerParameter(description: "The conversation id to add the provided dataset", Required = true)]
			Guid conversationId,
			[FromRoute]
			[SwaggerParameter(description: "The dataset id to add the provided conversation", Required = true)]
			Guid datasetId,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("adding").And("conversationId", conversationId).And("datasetId", datasetId).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);
			Boolean ownedConverastionFound = await this._queryFactory.Query<ConversationQuery>().Authorize(AuthorizationFlags.Any).Ids(conversationId).UserIds(userId.Value).AnyAsync();
			if (!ownedConverastionFound) throw new DGNotFoundException(this._localizer["general_notFound", conversationId, nameof(App.Model.Conversation)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationDatasetCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Conversation persisted = await this._conversationService.AddAsync(conversationId, datasetId, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.Conversation.AsAccountable());

			return persisted;
		}

		[HttpDelete("dataset/me/{conversationId}/{datasetId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Remove dataset from owned conversation")]
		[SwaggerResponse(statusCode: 200, description: "The dataset was removed and returns the updated conversation", type: typeof(App.Model.Conversation))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.Conversation> RemoveDatasetFromConversation(
			[FromRoute]
			[SwaggerParameter(description: "The conversation id from which to remove the provided dataset", Required = true)]
			Guid conversationId,
			[FromRoute]
			[SwaggerParameter(description: "The dataset id to remove from the provided conversation", Required = true)]
			Guid datasetId,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("adding").And("conversationId", conversationId).And("datasetId", datasetId).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);
			Boolean conversationFound = await this._queryFactory.Query<ConversationQuery>().Authorize(AuthorizationFlags.Any).Ids(conversationId).UserIds(userId.Value).AnyAsync();
			if (!conversationFound) throw new DGNotFoundException(this._localizer["general_notFound", conversationId, nameof(App.Model.Conversation)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<ConversationDatasetCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Conversation persisted = await this._conversationService.RemoveAsync(conversationId, datasetId, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.Conversation.AsAccountable());

			return persisted;
		}

		[HttpDelete("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Deletes the user conversation by id")]
		[SwaggerResponse(statusCode: 200, description: "Use conversation deleted")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		public async Task Delete(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to delete", Required = true)]
			Guid id)
		{
			this._logger.Debug(new MapLogEntry("delete").And("type", nameof(App.Model.Conversation)).And("id", id));

			await this._conversationService.DeleteAsync(id);

			this._accountingService.AccountFor(KnownActions.Delete, KnownResources.Conversation.AsAccountable());
		}
	}
}
