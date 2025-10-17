using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model.Lookup;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.UserCollection;
using DataGEMS.Gateway.Api.Transaction;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/user/collection")]
	public class UserCollectionController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<UserCollectionController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IUserCollectionService _userCollectionService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public UserCollectionController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<UserCollectionController> logger,
			IAccountingService accountingService,
			IAuthorizationContentResolver authorizationContentResolver,
			IUserCollectionService userCollectionService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ErrorThesaurus errors)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._userCollectionService = userCollectionService;
			this._localizer = localizer;
			this._errors = errors;
		}

		[HttpPost("query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserCollectionLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query user collections")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching user collections along with the count", type: typeof(QueryResult<App.Model.UserCollection>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.UserCollection>> Query(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			UserCollectionLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.UserCollection)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollectionQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<App.Data.UserCollection> datas = await query.CollectAsync(censoredFields);
			List<App.Model.UserCollection> models = await this._builderFactory.Builder<UserCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserCollection.AsAccountable());

			return new QueryResult<App.Model.UserCollection>(models, count);
		}

		[HttpPost("me/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserCollectionLookup.QueryMeValidator), "lookup")]
		[SwaggerOperation(Summary = "Query user owned collections")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching user owned collections along with the count", type: typeof(QueryResult<App.Model.UserCollection>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.UserCollection>> MeQuery(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			UserCollectionLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.UserCollection)).And("lookup", lookup));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(lookup.Project, CensorContext.AsCensor(), userId);
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollectionQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any).UserIds(userId.Value);
			List<App.Data.UserCollection> datas = await query.CollectAsync(censoredFields);
			List<App.Model.UserCollection> models = await this._builderFactory.Builder<UserCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserCollection.AsAccountable());

			return new QueryResult<App.Model.UserCollection>(models, count);
		}

		[HttpGet("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup user collection by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching user collection", type: typeof(QueryResult<App.Model.UserCollection>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.UserCollection> Get(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.UserCollection)).And("id", id).And("fields", fieldSet));

			Guid userId = await this._queryFactory.Query<UserCollectionQuery>().Authorize(AuthorizationFlags.None).Ids(id).FirstAsync(x => x.UserId);
			if (userId == default(Guid)) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.UserCollection)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollectionQuery query = this._queryFactory.Query<UserCollectionQuery>().Ids(id).DisableTracking().Authorize(AuthorizationFlags.Any);
			App.Data.UserCollection data = await query.FirstAsync(censoredFields);
			App.Model.UserCollection model = await this._builderFactory.Builder<UserCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.UserCollection)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserCollection.AsAccountable());

			return model;
		}

		[HttpPost("me/persist")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserCollectionPersist.PersistValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Persist user owned collection")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user owned collection", type: typeof(App.Model.UserCollection))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<UserCollection> Persist(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			UserCollectionPersist model, 
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserCollectionPersist)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollection persisted = await this._userCollectionService.PersistAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.UserCollection.AsAccountable());

			return persisted;
		}

		[HttpPost("me/persist/deep")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserCollectionPersistDeep.PersistValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Persist user owned collection along with details provided")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user owned collection", type: typeof(App.Model.UserCollection))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<UserCollection> PersistDeep(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			UserCollectionPersistDeep model,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserCollectionPersistDeep)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollection persisted = await this._userCollectionService.PersistAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.UserCollection.AsAccountable());

			return persisted;
		}

		[HttpPost("me/patch/dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserCollectionDatasetPatch.PatchValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Patch user owned collection with updated datasets")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user owned collection", type: typeof(App.Model.UserCollection))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<UserCollection> PatchDatasets(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			UserCollectionDatasetPatch model,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("patching").And("type", nameof(App.Model.UserCollectionDatasetPatch)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollection persisted = await this._userCollectionService.PatchAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.UserCollection.AsAccountable());

			return persisted;
		}

		[HttpPost("dataset/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserDatasetCollectionLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query user collection datasets")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching user collection datasets along with the count", type: typeof(QueryResult<App.Model.UserDatasetCollection>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.UserDatasetCollection>> QueryDataset(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			UserDatasetCollectionLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.UserDatasetCollection)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<UserDatasetCollectionCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserDatasetCollectionQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<App.Data.UserDatasetCollection> datas = await query.CollectAsync(censoredFields);
			List<App.Model.UserDatasetCollection> models = await this._builderFactory.Builder<UserDatasetCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserCollection.AsAccountable());

			return new QueryResult<App.Model.UserDatasetCollection>(models, count);
		}

		[HttpPost("dataset/me/query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserDatasetCollectionLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query user owned collection datasets")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching user owned collection datasets along with the count", type: typeof(QueryResult<App.Model.UserDatasetCollection>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.UserDatasetCollection>> MeQueryDataset(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			UserDatasetCollectionLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.UserDatasetCollection)).And("lookup", lookup));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserDatasetCollectionCensor>().Censor(lookup.Project, CensorContext.AsCensor(), userId);
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserDatasetCollectionQuery query = lookup.Enrich(this._queryFactory)
				.DisableTracking()
				.Authorize(AuthorizationFlags.Any)
				.UserCollectionSubQuery(
					this._queryFactory.Query<UserCollectionQuery>()
					.Authorize(AuthorizationFlags.Any)
					.UserIds(userId.Value));
			List<App.Data.UserDatasetCollection> datas = await query.CollectAsync(censoredFields);
			List<App.Model.UserDatasetCollection> models = await this._builderFactory.Builder<UserDatasetCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserCollection.AsAccountable());

			return new QueryResult<App.Model.UserDatasetCollection>(models, count);
		}

		[HttpGet("dataset/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup user collection dataset by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching user collection dataset", type: typeof(QueryResult<App.Model.UserDatasetCollection>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.UserDatasetCollection> GetDataset(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.UserDatasetCollection)).And("id", id).And("fields", fieldSet));

			Guid userId = await this._queryFactory.Query<UserCollectionQuery>()
				.Authorize(AuthorizationFlags.None)
				.IsActive(IsActive.Active)
				.UserDatasetCollectionSubQuery(
					this._queryFactory.Query<UserDatasetCollectionQuery>()
					.Authorize(AuthorizationFlags.None)
					.IsActive(IsActive.Active)
					.Ids(id))
				.FirstAsync(x => x.UserId);
			if (userId == default(Guid)) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.UserDatasetCollection)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserDatasetCollectionCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserDatasetCollectionQuery query = this._queryFactory.Query<UserDatasetCollectionQuery>().Ids(id).DisableTracking().Authorize(AuthorizationFlags.Any);
			App.Data.UserDatasetCollection data = await query.FirstAsync(censoredFields);
			App.Model.UserDatasetCollection model = await this._builderFactory.Builder<UserDatasetCollectionBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.UserDatasetCollection)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserCollection.AsAccountable());

			return model;
		}

		[HttpPost("dataset/me/{userCollectionId}/{datasetId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Add dataset in owned user collection")]
		[SwaggerResponse(statusCode: 200, description: "The dataset was added and returns the updated user collection", type: typeof(App.Model.UserCollection))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.UserCollection> AddDatasetInUserCollection(
			[FromRoute]
			[SwaggerParameter(description: "The user collection id to add the provided dataset", Required = true)]
			Guid userCollectionId,
			[FromRoute]
			[SwaggerParameter(description: "The dataset id to add the provided user collection", Required = true)]
			Guid datasetId,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("adding").And("userCollectionId", userCollectionId).And("datasetId", datasetId).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);
			Boolean ownedCollectionFound = await this._queryFactory.Query<UserCollectionQuery>().Authorize(AuthorizationFlags.Any).Ids(userCollectionId).UserIds(userId.Value).AnyAsync();
			if (!ownedCollectionFound) throw new DGNotFoundException(this._localizer["general_notFound", userCollectionId, nameof(App.Model.UserCollection)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollection persisted = await this._userCollectionService.AddAsync(userCollectionId, datasetId, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.UserCollection.AsAccountable());

			return persisted;
		}

		[HttpDelete("dataset/me/{userCollectionId}/{datasetId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Remove dataset from owned user collection")]
		[SwaggerResponse(statusCode: 200, description: "The dataset was removed and returns the updated user collection", type: typeof(App.Model.UserCollection))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.UserCollection> RemoveDatasetFromUserCollection(
			[FromRoute]
			[SwaggerParameter(description: "The user collection id from which to remove the provided dataset", Required = true)]
			Guid userCollectionId,
			[FromRoute]
			[SwaggerParameter(description: "The dataset id to remove from the provided user collection", Required = true)]
			Guid datasetId,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("adding").And("userCollectionId", userCollectionId).And("datasetId", datasetId).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);
			Boolean ownedCollectionFound = await this._queryFactory.Query<UserCollectionQuery>().Authorize(AuthorizationFlags.Any).Ids(userCollectionId).UserIds(userId.Value).AnyAsync();
			if (!ownedCollectionFound) throw new DGNotFoundException(this._localizer["general_notFound", userCollectionId, nameof(App.Model.UserCollection)]);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCollectionCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserCollection persisted = await this._userCollectionService.RemoveAsync(userCollectionId, datasetId, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.UserCollection.AsAccountable());

			return persisted;
		}

		[HttpDelete("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Deletes the user collection by id")]
		[SwaggerResponse(statusCode: 200, description: "User collection deleted")]
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
			this._logger.Debug(new MapLogEntry("delete").And("type", nameof(App.Model.UserCollection)).And("id", id));

			await this._userCollectionService.DeleteAsync(id);

			this._accountingService.AccountFor(KnownActions.Delete, KnownResources.UserCollection.AsAccountable());
		}
	}
}
