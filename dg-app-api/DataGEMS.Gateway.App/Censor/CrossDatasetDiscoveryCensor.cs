using Cite.Tools.Auth.Claims;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Censor;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Censor
{
	public class CrossDatasetDiscoveryCensor : ICensor
	{
		private readonly CensorFactory _censorFactory;
		private readonly IAuthorizationService _authService;
		private readonly ILogger<CrossDatasetDiscoveryCensor> _logger;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ICurrentPrincipalResolverService _principalResolverService;
		private readonly ClaimExtractor _claimExtractor;

		public CrossDatasetDiscoveryCensor(
			CensorFactory censorFactory,
			IAuthorizationService authService,
			ILogger<CrossDatasetDiscoveryCensor> logger,
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
			this._logger.Debug(new MapLogEntry("censoring").And("type", nameof(Model.CrossDatasetDiscovery)).And("fields", fields).And("context", context));
			if (fields == null || fields.IsEmpty()) return null;

			IFieldSet censored = new FieldSet();
			Boolean authZPass = false;
			switch (context?.Behavior)
			{
				case CensorBehavior.Censor: { authZPass = await this._authService.Authorize(Permission.CanExecuteCrossDatasetDiscovery); break; }
				case CensorBehavior.Throw:
				default: { authZPass = await this._authService.AuthorizeForce(Permission.CanExecuteCrossDatasetDiscovery); break; }
			}
			if (authZPass)
			{
				censored = censored.Merge(fields.ExtractNonPrefixed());
			}

			censored = censored.MergeAsPrefixed(await this._censorFactory.Censor<DatasetCensor>().Censor(fields.ExtractPrefixed(nameof(Model.CrossDatasetDiscovery.Dataset).AsIndexerPrefix()), context), nameof(Model.CrossDatasetDiscovery.Dataset));

			return censored;
		}
	}
}
