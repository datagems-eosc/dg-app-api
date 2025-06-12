using Cite.Tools.Data.Censor;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Model.Lookup;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Censor;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/dataset")]
	public class DatasetController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly ILogger<DatasetController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly ErrorThesaurus _errors;

		public DatasetController(
			CensorFactory censorFactory,
			ILogger<DatasetController> logger,
			IAccountingService accountingService,
			ErrorThesaurus errors)
		{
			this._censorFactory = censorFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._errors = errors;
		}

		[HttpPost("query")]
		[Authorize]
		[ModelStateValidationFilter]
		public async Task<QueryResult<App.Model.Dataset>> Query([FromBody] DatasetLookup lookup)
		{
			this._logger.Debug(new MapLogEntry("querying").And("type", nameof(App.Model.Dataset)).And("lookup", lookup));

			IFieldSet censoredFields = await this._censorFactory.Censor<DatasetCensor>().Censor(lookup.Project, CensorContext.AsCensor());
			if (lookup.Project.CensoredAsUnauthorized(censoredFields)) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			//ActivityTypeQuery query = lookup.Enrich(this._queryFactory, AuthorizationFlags.Any).DisableTracking().Authorize(Authorization.AuthorizationFlags.Any);
			//List<ActivityType> models = await this._queryingService.CollectAsync(query, this._builderFactory.Builder<ActivityTypeBuilder>().Authorize(Authorization.AuthorizationFlags.Any), censoredFields);
			//int count = (lookup.Metadata != null && lookup.Metadata.CountAll) ? await this._queryingService.CountAsync(query) : models.Count;

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.Datasets.AsAccountable());

			List<App.Model.Dataset> models = null;
			int count = 0;

			return new QueryResult<App.Model.Dataset>(models, count);
		}
	}
}
