using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Censor;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Censor
{
	public class DeconstructedDatasetAnalyticalPatternCensor : ICensor
	{
		private readonly IAuthorizationService _authService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ILogger<DeconstructedDatasetAnalyticalPatternCensor> _logger;

		public DeconstructedDatasetAnalyticalPatternCensor(
			IAuthorizationService authService,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<DeconstructedDatasetAnalyticalPatternCensor> logger)
		{
			this._authService = authService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._logger = logger;
		}

		//GOTCHA: the properties are part of the dataset profile, so the same authorization as browsing the dataset applies (see DatasetCensor)
		public async Task<IFieldSet> Censor(IFieldSet fields, CensorContext context, Guid datasetId)
		{
			this._logger.Debug(new MapLogEntry("censoring").And("type", nameof(Model.DeconstructedDatasetAnalyticalPattern)).And("fields", fields).And("context", context).And("datasetId", datasetId));
			if (fields == null || fields.IsEmpty()) return null;

			IEnumerable<string> contextRoles = await this._authorizationContentResolver.EffectiveContextRolesForDatasetOfUser(datasetId);

			IFieldSet censored = new FieldSet();
			Boolean authZPass = false;
			switch (context?.Behavior)
			{
				case CensorBehavior.Censor: { authZPass = await this._authService.AuthorizeOrAffiliatedContext(new AffiliatedContextResource(contextRoles), Permission.BrowseDataset); break; }
				case CensorBehavior.Throw:
				default: { authZPass = await this._authService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(contextRoles), Permission.BrowseDataset); break; }
			}
			if (authZPass)
			{
				censored = censored.Merge(fields.ExtractNonPrefixed());
				censored = censored.MergeAsPrefixed(fields.ExtractPrefixed(nameof(Model.DeconstructedDatasetAnalyticalPattern.Dataset).AsIndexerPrefix()), nameof(Model.DeconstructedDatasetAnalyticalPattern.Dataset));
				censored = censored.MergeAsPrefixed(fields.ExtractPrefixed(nameof(Model.DeconstructedDatasetAnalyticalPattern.Distributions).AsIndexerPrefix()), nameof(Model.DeconstructedDatasetAnalyticalPattern.Distributions));
				censored = censored.MergeAsPrefixed(fields.ExtractPrefixed(nameof(Model.DeconstructedDatasetAnalyticalPattern.RecordSets).AsIndexerPrefix()), nameof(Model.DeconstructedDatasetAnalyticalPattern.RecordSets));
			}

			return censored;
		}
	}
}
