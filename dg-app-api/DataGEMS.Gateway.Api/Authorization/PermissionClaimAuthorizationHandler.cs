using Cite.Tools.Auth.Claims;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	public class PermissionClaimAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
	{
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly ILogger<PermissionClaimAuthorizationHandler> _logger;
		private readonly ClaimExtractor _extractor;
		private readonly PermissionPolicyConfig _config;

		public PermissionClaimAuthorizationHandler(
			IPermissionPolicyService permissionPolicyService,
			ILogger<PermissionClaimAuthorizationHandler> logger,
			ClaimExtractor extractor,
			PermissionPolicyConfig config)
		{
			this._logger = logger;
			this._permissionPolicyService = permissionPolicyService;
			this._extractor = extractor;
			this._config = config;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
		{
			if (context.User == null || !context.User.Claims.Any())
			{
				this._logger.Trace("current user not set");
				return Task.CompletedTask;
			}

			if (!requirement.RequiredPermissions.Any())
			{
				this._logger.Trace("no requirements specified");
				return Task.CompletedTask;
			}

			if (this._config.ExtendedClaims == null || this._config.ExtendedClaims.Count == 0)
			{
				this._logger.Trace("no claims configured");
				return Task.CompletedTask;
			}

			int hitCount = 0;
			foreach (String permission in requirement.RequiredPermissions)
			{
				foreach (String claim in this._config.ExtendedClaims)
				{
					List<String> claimValues = this._extractor.AsStrings(context.User, claim);
					Boolean hasPermission = this.HasPermission(permission, claim, claimValues);
					if (hasPermission)
					{
						hitCount += 1;
						break;
					}
				}
			}

			this._logger.Trace("required {allcount} permissions, current principal has matched {hascount} and require all is set to: {matchall}", requirement.RequiredPermissions?.Count, hitCount, requirement.MatchAll);

			if ((requirement.MatchAll && requirement.RequiredPermissions.Count == hitCount) ||
				!requirement.MatchAll && hitCount > 0) context.Succeed(requirement);

			return Task.CompletedTask;
		}

		private Boolean HasPermission(String permission, String claim, List<String> values)
		{
			ISet<String> permissionClaimValues = this._permissionPolicyService.ClaimsHaving(claim, permission);
			Boolean hasClaimValue = values.Any(x => permissionClaimValues.Contains(x));
			return hasClaimValue;
		}
	}
}
