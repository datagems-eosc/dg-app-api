using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cite.Tools.Auth.Claims;
using Cite.Tools.Data.Censor;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Censor
{
	public class WorkflowTaskLogsCensor : ICensor
	{
		private readonly CensorFactory _censorFactory;
		private readonly IAuthorizationService _authService;
		private readonly ILogger<WorkflowTaskLogsCensor> _logger;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ICurrentPrincipalResolverService _principalResolverService;
		private readonly ClaimExtractor _claimExtractor;

		public WorkflowTaskLogsCensor(
			CensorFactory censorFactory,
			IAuthorizationService authService,
			ILogger<WorkflowTaskLogsCensor> logger,
			IAuthorizationContentResolver authorizationContentResolver,
			ICurrentPrincipalResolverService principalResolverService,
			ClaimExtractor claimExtractor)
		{
			this._logger = logger;
			this._censorFactory = censorFactory;
			this._authService = authService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._principalResolverService = principalResolverService;
			this._claimExtractor = claimExtractor;
		}

		public async Task<IFieldSet> Censor(IFieldSet fields, CensorContext context)
		{
			this._logger.Debug(new MapLogEntry("censoring").And("type", nameof(Model.Collection)).And("fields", fields).And("context", context));
			if (fields == null || fields.IsEmpty()) return null;

			IFieldSet censored = new FieldSet();
			Boolean authZPass = false;
			switch (context?.Behavior)
			{
				case CensorBehavior.Censor: { authZPass = await this._authService.Authorize(Permission.BrowseWorkflowExecution); break; }
				case CensorBehavior.Throw:
				default: { authZPass = await this._authService.AuthorizeForce(Permission.BrowseWorkflowExecution); break; }
			}
			if (authZPass)
			{
				censored = censored.Merge(fields.ExtractNonPrefixed());
			}

			return censored;
		}
	}
}
