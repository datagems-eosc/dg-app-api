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
	public class ConversationMessageCensor : ICensor
	{
		private readonly CensorFactory _censorFactory;
		private readonly IAuthorizationService _authService;
		private readonly ILogger<ConversationMessageCensor> _logger;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ICurrentPrincipalResolverService _principalResolverService;
		private readonly ClaimExtractor _claimExtractor;

		public ConversationMessageCensor(
			CensorFactory censorFactory,
			IAuthorizationService authService,
			ILogger<ConversationMessageCensor> logger,
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

		public async Task<IFieldSet> Censor(IFieldSet fields, CensorContext context, Guid? userId = null)
		{
			this._logger.Debug(new MapLogEntry("censoring").And("type", nameof(Model.ConversationMessage)).And("fields", fields).And("context", context).And("userId", userId));
			if (fields == null || fields.IsEmpty()) return null;

			String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(userId);

			IFieldSet censored = new FieldSet();
			Boolean authZPass = false;
			switch (context?.Behavior)
			{
				case CensorBehavior.Censor: { authZPass = await this._authService.AuthorizeOrOwner(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null, Permission.BrowseConversationMessage); break; }
				case CensorBehavior.Throw:
				default: { authZPass = await this._authService.AuthorizeOrOwnerForce(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null, Permission.BrowseConversationMessage); break; }
			}
			if (authZPass)
			{
				censored = censored.Merge(fields.ExtractNonPrefixed());
			}

			censored = censored.MergeAsPrefixed(await this._censorFactory.Censor<ConversationCensor>().Censor(fields.ExtractPrefixed(nameof(Model.ConversationMessage.Conversation).AsIndexerPrefix()), context, userId), nameof(Model.ConversationMessage.Conversation));

			return censored;
		}
	}
}
