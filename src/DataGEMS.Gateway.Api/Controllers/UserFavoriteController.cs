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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/user/favorites")]
	[ApiController]
	public class UserFavoriteController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<UserFavoriteController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IUserFavoriteService _userFavoriteService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public UserFavoriteController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<UserFavoriteController> logger,
			IAccountingService accountingService,
			IAuthorizationContentResolver authorizationContentResolver,
			IUserFavoriteService userFavoriteService,
			ErrorThesaurus errors,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._userFavoriteService = userFavoriteService;
			this._errors = errors;
			this._localizer = localizer;
		}

		[HttpGet]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup user favorites")]
		[SwaggerResponse(statusCode: 200, description: "The matching user favorites", type: typeof(QueryResult<App.Model.UserFavorite>))]
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

			UserFavoriteQuery query = this._queryFactory.Query<UserFavoriteQuery>().DisableTracking().Authorize(AuthorizationFlags.Owner).IsActive(IsActive.Active);
			List<App.Data.UserFavorite> data = await query.CollectAsync(censoredFields);
			var models = await this._builderFactory.Builder<UserFavoriteBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.UserFavorite.AsAccountable());

			return models;
		}

		[HttpPost("persist")]
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

		[HttpDelete("{id}")]
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
			[SwaggerParameter(description: "The id of the item to delete", Required = true)]
			Guid id)
		{
			this._logger.Debug(new MapLogEntry("delete").And("type", nameof(App.Model.UserFavorite)).And("id", id));

			await this._userFavoriteService.DeleteAsync(id);

			this._accountingService.AccountFor(KnownActions.Delete, KnownResources.UserFavorite.AsAccountable());
		}
	}
}
