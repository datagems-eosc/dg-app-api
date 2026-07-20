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
	public class QueryDisambiguationCensor : ICensor
	{
		private readonly CensorFactory _censorFactory;
		private readonly IAuthorizationService _authService;
		private readonly ILogger<QueryDisambiguationCensor> _logger;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ICurrentPrincipalResolverService _principalResolverService;
		private readonly ClaimExtractor _claimExtractor;

		public QueryDisambiguationCensor(
			CensorFactory censorFactory,
			IAuthorizationService authService,
			ILogger<QueryDisambiguationCensor> logger,
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

		public async Task<IFieldSet> Censor(IFieldSet fields, CensorContext context, IEnumerable<Guid> datasetIds)
		{
			this._logger.Debug(new MapLogEntry("censoring").And("type", nameof(App.Model.QueryDisambiguation)).And("fields", fields).And("context", context));
			if (fields == null || fields.IsEmpty() || datasetIds == null || !datasetIds.Any()) return null;

			List<string> contextRoles = await _authorizationContentResolver.ContextRolesOf();

			IFieldSet censored = new FieldSet();
			bool authZPass = false;
			switch (context?.Behavior)
			{
				case CensorBehavior.Censor: { authZPass = await this._authService.AuthorizeOrAffiliatedContext(new AffiliatedContextResource(contextRoles), Permission.CanDisambiguate); break; }
				case CensorBehavior.Throw:
				default: { authZPass = await this._authService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(contextRoles), Permission.CanDisambiguate); break; }
			}

			List<Guid> allowMetadataProjectionDatasetIds = await this._authorizationContentResolver.EffectiveContextAffiliatedDatasets(Permission.CanPowerDisambiguate);
			Boolean blockMetadata = datasetIds.Any(x=> !allowMetadataProjectionDatasetIds.Contains(x));
			if (authZPass)
			{
				censored = censored.Merge(fields.ExtractNonPrefixed());
				if(blockMetadata && 
					censored.HasField(nameof(App.Model.QueryDisambiguationViewModel.Metadata))) 
					censored.Fields = censored.Fields.Where(x => !x.Equals(nameof(App.Model.QueryDisambiguationViewModel.Metadata), StringComparison.CurrentCultureIgnoreCase)).ToHashSet();
			}

			return censored;
		}
	}
}
