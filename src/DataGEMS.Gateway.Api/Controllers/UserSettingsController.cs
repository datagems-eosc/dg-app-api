using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model;
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
using DataGEMS.Gateway.App.Service.UserFavorite;
using DataGEMS.Gateway.App.Service.UserSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/user/settings")]
	public class UserSettingsController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<UserSettingsController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IUserFavoriteService _userFavoriteService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public UserSettingsController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<UserSettingsController> logger,
			IAccountingService accountingService,
			IUserSettingsService userSettingsService,
			IUserFavoriteService userFavoriteService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			IAuthorizationContentResolver authorizationContentResolver,
			ErrorThesaurus errors)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._userSettingsService = userSettingsService;
			this._userFavoriteService = userFavoriteService;
			this._localizer = localizer;
			this._errors = errors;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		[HttpGet("id/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup user settings by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching user settings", type: typeof(App.Model.UserSettings))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.UserSettings> GetById(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.UserSettings)).And("id", id).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserSettingsCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserSettingsQuery query = this._queryFactory.Query<UserSettingsQuery>().Ids(id).UserIds(userId.Value).DisableTracking().Authorize(AuthorizationFlags.Any);
			App.Data.UserSettings data = await query.FirstAsync(censoredFields);
			App.Model.UserSettings model = await this._builderFactory.Builder<UserSettingsBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.UserSettings)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserSettings.AsAccountable());

			return model;
		}

		[HttpGet("key/{key}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup user settings by key")]
		[SwaggerResponse(statusCode: 200, description: "The matching user settings", type: typeof(List<App.Model.UserSettings>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Model.UserSettings>> GetByKey(
			[FromRoute]
			[SwaggerParameter(description: "The key of the item to lookup", Required = true)]
			String key,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.UserSettings)).And("key", key).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserSettingsCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserSettingsQuery query = this._queryFactory.Query<UserSettingsQuery>().Keys(key).UserIds(userId.Value).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<App.Data.UserSettings> datas = await query.CollectAsync(censoredFields);
			List<App.Model.UserSettings> models = await this._builderFactory.Builder<UserSettingsBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserSettings.AsAccountable());

			return models;
		}

		[HttpPost("persist")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserSettingsPersist.PersistValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Persist user settings")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user settings", type: typeof(App.Model.UserSettings))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<UserSettings> Persist(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			UserSettingsPersist model,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserSettingsPersist)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserSettingsCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserSettings persisted = await this._userSettingsService.PersistAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.UserSettings.AsAccountable());

			return persisted;
		}

		[HttpDelete("id/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Deletes the user settings by id")]
		[SwaggerResponse(statusCode: 200, description: "User settings deleted")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		public async Task DeleteById(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to delete", Required = true)]
			Guid id)
		{
			this._logger.Debug(new MapLogEntry("delete").And("type", nameof(App.Model.UserSettings)).And("id", id));

			await this._userSettingsService.DeleteAsync(id);

			this._accountingService.AccountFor(KnownActions.Delete, KnownResources.UserSettings.AsAccountable());
		}

		[HttpDelete("key/{key}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Deletes the user settings by key")]
		[SwaggerResponse(statusCode: 200, description: "User settings deleted")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		public async Task DeleteByKey(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to delete", Required = true)]
			String key)
		{
			this._logger.Debug(new MapLogEntry("delete").And("type", nameof(App.Model.UserSettings)).And("key", key));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			UserSettingsQuery query = this._queryFactory.Query<UserSettingsQuery>().Keys(key).UserIds(userId.Value).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<Guid> ids = await query.CollectAsync(x=> x.Id);

			await this._userSettingsService.DeleteAsync(ids);

			this._accountingService.AccountFor(KnownActions.Delete, KnownResources.UserSettings.AsAccountable());
		}

		[HttpGet("favorites/dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup user dataset favorites")]
		[SwaggerResponse(statusCode: 200, description: "The matching user favorites", type: typeof(List<App.Model.UserFavorite>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Model.UserFavorite>> Get(
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.UserFavorite)).And("fields", fieldSet));

			Guid userId = (await this._authorizationContentResolver.CurrentUserId()).Value;
			IFieldSet censoredFields = await this._censorFactory.Censor<UserFavoriteCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserFavoriteQuery query = this._queryFactory.Query<UserFavoriteQuery>().DisableTracking().Authorize(AuthorizationFlags.Any).IsActive(IsActive.Active);
			List<App.Data.UserFavorite> data = await query.CollectAsync(censoredFields);
			List<App.Model.UserFavorite> models = await this._builderFactory.Builder<UserFavoriteBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserFavorite.AsAccountable());

			return models;
		}

		[HttpPost("favorites/dataset/persist")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserFavoritePersist.PersistValidator), "model")]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Persist user favorite")]
		[SwaggerResponse(statusCode: 200, description: "The persisted user favorite", type: typeof(App.Model.UserFavorite))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<UserFavorite> Persist(
			[FromBody]
			[SwaggerRequestBody(description: "The model to persist", Required = true)]
			UserFavoritePersist model,
			[FromQuery]
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserFavoritePersist)).And("fields", fieldSet));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGApplicationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			IFieldSet censoredFields = await this._censorFactory.Censor<UserFavoriteCensor>().Censor(fieldSet, CensorContext.AsCensor(), userId);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserFavorite persisted = await this._userFavoriteService.PersistAsync(model, censoredFields);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.UserFavorite.AsAccountable());

			return persisted;
		}

		[HttpDelete("favorites/dataset/{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Deletes the user favorite by id")]
		[SwaggerResponse(statusCode: 200, description: "User favorite deleted")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		public async Task Delete(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to delete or the datasetId", Required = true)]
			Guid id)
		{
			this._logger.Debug(new MapLogEntry("delete").And("type", nameof(App.Model.UserFavorite)).And("id", id));

			await this._userFavoriteService.DeleteByIdOrDatasetIdAsync(id);

			this._accountingService.AccountFor(KnownActions.Delete, KnownResources.UserFavorite.AsAccountable());
		}
	}
}
