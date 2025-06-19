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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.Api.OpenApi;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/user")]
	public class UserController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<UserController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public UserController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<UserController> logger,
			IAccountingService accountingService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			ErrorThesaurus errors)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._localizer = localizer;
			this._errors = errors;
		}

		[HttpPost("query")]
		[Authorize]
		[ModelStateValidationFilter]
		[ValidationFilter(typeof(UserLookup.QueryValidator), "lookup")]
		[SwaggerOperation(Summary = "Query users")]
		[SwaggerResponse(statusCode: 200, description: "The list of matching users along with the count", type: typeof(QueryResult<App.Model.User>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<QueryResult<App.Model.User>> Query(
			[FromBody]
			[SwaggerRequestBody(description: "The query predicates", Required = true)]
			UserLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.User)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<App.Data.User> datas = await query.CollectAsync(lookup.Project);
			List<App.Model.User> models = await this._builderFactory.Builder<UserBuilder>().Authorize(AuthorizationFlags.Any).Build(lookup.Project, datas);
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.User.AsAccountable());

			return new QueryResult<App.Model.User>(models, count);
		}

		[HttpGet("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Lookup user by id")]
		[SwaggerResponse(statusCode: 200, description: "The matching user", type: typeof(QueryResult<App.Model.User>))]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 404, description: "Could not locate item with the provided id")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<App.Model.User> Get(
			[FromRoute]
			[SwaggerParameter(description: "The id of the item to lookup", Required = true)]
			Guid id,
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = true)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.User)).And("id", id).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<UserCensor>().Censor(fieldSet, CensorContext.AsCensor(), id);
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			UserQuery query = this._queryFactory.Query<UserQuery>().Ids(id).DisableTracking().Authorize(AuthorizationFlags.Any);
			App.Data.User data = await query.FirstAsync(fieldSet);
			App.Model.User model = await this._builderFactory.Builder<UserBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.User)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.User.AsAccountable());

			return model;
		}
	}
}
