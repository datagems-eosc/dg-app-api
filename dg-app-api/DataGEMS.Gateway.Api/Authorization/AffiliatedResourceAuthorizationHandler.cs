using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	public class AffiliatedResourceAuthorizationHandler : AuthorizationHandler<AffiliatedResourceRequirement, AffiliatedResource>
	{
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly ILogger<AffiliatedResourceAuthorizationHandler> _logger;

		public AffiliatedResourceAuthorizationHandler(
			ILogger<AffiliatedResourceAuthorizationHandler> logger,
			IPermissionPolicyService permissionPolicyService)
		{
			this._logger = logger;
			this._permissionPolicyService = permissionPolicyService;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AffiliatedResourceRequirement requirement, AffiliatedResource resource)
		{
			if (context.User == null || !context.User.Claims.Any())
			{
				this._logger.Trace("current user not set");
				return Task.CompletedTask;
			}
			if (resource.UserIds == null || !resource.UserIds.Any())
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
			ISet<String> affiliatedRolePermissions = this._permissionPolicyService.PermissionsOfAffiliated(resource.AffiliatedRoles);
			if (resource.AffiliatedPermissions != null && resource.AffiliatedPermissions.Any()) affiliatedPermissions = affiliatedRolePermissions.Union(resource.AffiliatedPermissions).ToHashSet();
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
