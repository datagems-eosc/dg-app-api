using Microsoft.AspNetCore.Authorization;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	public class AffiliatedDeferredAuthorizationHandler : AuthorizationHandler<AffiliatedDeferredAuthorizationRequirement, AffiliatedDeferredResource>
	{
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly ILogger<AffiliatedDeferredAuthorizationHandler> _logger;

		public AffiliatedDeferredAuthorizationHandler(
			IPermissionPolicyService permissionPolicyService,
			ILogger<AffiliatedDeferredAuthorizationHandler> logger)
		{
			this._logger = logger;
			this._permissionPolicyService = permissionPolicyService;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AffiliatedDeferredAuthorizationRequirement requirement, AffiliatedDeferredResource contextResource)
		{
			if (context.User == null || !context.User.Claims.Any())
			{
				this._logger.Trace("current user not set");
				return Task.CompletedTask;
			}
			if (contextResource.UserIds == null || !contextResource.UserIds.Any())
			{
				this._logger.Trace("resource users not set");
				return Task.CompletedTask;
			}

			if (!requirement.RequiredPermissions.Any())
			{
				this._logger.Trace("no requirements specified");
				return Task.CompletedTask;
			}

			ISet<String> affiliatedPermissions = null;
			ISet<String> affiliatedRolePermissions = this._permissionPolicyService.PermissionsOfAffiliated(contextResource.AffiliatedRoles);
			if (contextResource.AffiliatedPermissions != null && contextResource.AffiliatedPermissions.Any()) affiliatedPermissions = affiliatedRolePermissions.Union(contextResource.AffiliatedPermissions).ToHashSet();
			else affiliatedPermissions = affiliatedRolePermissions;

			int hitCount = 0;
			foreach (String permission in requirement.RequiredPermissions)
			{
				Boolean hasAffiliatedPermission = affiliatedPermissions.Contains(permission);
				if (hasAffiliatedPermission) hitCount += 1;
			}

			this._logger.Trace("required {allcount} permissions, current principal has matched {hascount} and require all is set to: {matchall}", requirement.RequiredPermissions?.Count, hitCount, requirement.MatchAll);

			if ((requirement.MatchAll && requirement.RequiredPermissions.Count == hitCount) ||
				!requirement.MatchAll && hitCount > 0) context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}
}
