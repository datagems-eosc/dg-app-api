using Cite.Tools.Auth.Claims;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Censor;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace DataGEMS.Gateway.App.Censor
{
	public class DatasetCensor : ICensor
	{
		private readonly CensorFactory _censorFactory;
		private readonly IAuthorizationService _authService;
		private readonly ILogger<DatasetCensor> _logger;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ICurrentPrincipalResolverService _principalResolverService;
		private readonly ClaimExtractor _claimExtractor;

		public DatasetCensor(
			CensorFactory censorFactory,
			IAuthorizationService authService,
			ILogger<DatasetCensor> logger,
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

		public async Task<IFieldSet> Censor(IFieldSet fields, CensorContext context, Boolean? isNewItem = null)
		{
			this._logger.Debug(new MapLogEntry("censoring").And("type", nameof(Model.Dataset)).And("fields", fields).And("context", context));
			if (fields == null || fields.IsEmpty()) return null;

			List<string> contextRoles = await _authorizationContentResolver.ContextRolesOf();

			IFieldSet censored = new FieldSet();
			Boolean authZPass = false;

			if (isNewItem.HasValue && isNewItem.Value)
			{
				//GOTCHA: for new items AuthZ will be applied for specific action. Browse will not be granted before having the collection
				authZPass = true;
			}
			else
			{
				switch (context?.Behavior)
				{
					case CensorBehavior.Censor: { authZPass = await this._authService.AuthorizeOrAffiliatedContext(new AffiliatedContextResource(contextRoles), Permission.BrowseDataset); break; }
					case CensorBehavior.Throw:
					default: { authZPass = await this._authService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(contextRoles), Permission.BrowseDataset); break; }
				}
			}
			if (authZPass)
			{
				censored = censored.Merge(fields.ExtractNonPrefixed());
				censored = censored.MergeAsPrefixed(fields.ExtractPrefixed(nameof(Model.Dataset.Permissions).AsIndexerPrefix()), nameof(Model.Dataset.Permissions));
				censored = censored.MergeAsPrefixed(fields.ExtractPrefixed(nameof(Model.Dataset.DataLocation).AsIndexerPrefix()), nameof(Model.Dataset.DataLocation));
			}

			censored = censored.MergeAsPrefixed(await this._censorFactory.Censor<CollectionCensor>().Censor(fields.ExtractPrefixed(nameof(Model.Dataset.Collections).AsIndexerPrefix()), context), nameof(Model.Dataset.Collections));

			return censored;
		}
	}
}
