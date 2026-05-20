using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Censor;
using Cite.Tools.Data.Query;
using Cite.WebTools.Validation;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Service.DataManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/pilot")]
	[ApiController]
	public class PilotController : ControllerBase
	{
		private readonly CensorFactory _censorFactory;
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly ILogger<PilotController> _logger;
		private readonly IAccountingService _accountingService;
		private readonly IDataManagementService _datasetService;
		private readonly ErrorThesaurus _errors;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public PilotController(
			CensorFactory censorFactory,
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<PilotController> logger,
			IAccountingService accountingService,
			IDataManagementService datasetService,
			ErrorThesaurus errors,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer)
		{
			this._censorFactory = censorFactory;
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._logger = logger;
			this._accountingService = accountingService;
			this._datasetService = datasetService;
			this._errors = errors;
			this._localizer = localizer;
		}

	}
}
