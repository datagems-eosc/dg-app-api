using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model.Builder;
using DataGEMS.Gateway.App.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/dataset")]
	public class DatasetController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<DatasetController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public DatasetController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<DatasetController> logger,
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
		[ValidationFilter(typeof(DatasetLookup.QueryValidator), "lookup")]
		public async Task<QueryResult<App.Model.Dataset>> Query([FromBody] DatasetLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.Dataset)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DatasetLocalQuery query = lookup.Enrich(this._queryFactory).DisableTracking().Authorize(AuthorizationFlags.Any);
			List<DataGEMS.Gateway.App.DataManagement.Model.Dataset> datas = await query.CollectAsyncAsModels();
			int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await query.CountAsync() : datas.Count;
			List<App.Model.Dataset> models = await this._builderFactory.Builder<DatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, datas);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Dataset.AsAccountable());

			return new QueryResult<App.Model.Dataset>(models, count);
		}

		[HttpGet("{id}")]
		[Authorize]
		[ModelStateValidationFilter]
		public async Task<App.Model.Dataset> Get([FromRoute] Guid id, [ModelBinder(Name = "f")] IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("get").And("type", nameof(App.Model.Dataset)).And("id", id).And("fields", fieldSet));

			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(fieldSet, CensorContext.AsCensor());
			if (fieldSet.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			DatasetLocalQuery query = this._queryFactory.Query<DatasetLocalQuery>().Ids(id).DisableTracking().Authorize(AuthorizationFlags.Any);
			DataGEMS.Gateway.App.DataManagement.Model.Dataset data = await query.FirstAsyncAsModel();
			App.Model.Dataset model = await this._builderFactory.Builder<DatasetBuilder>().Authorize(AuthorizationFlags.Any).Build(censoredFields, data);
			if (model == null) throw new DGNotFoundException(this._localizer["general_notFound", id, nameof(App.Model.Dataset)]);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Dataset.AsAccountable());

			return model;
		}
	}
}
