using Microsoft.AspNetCore.Authorization;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	public class AffiliatedDatasetAuthorizationHandler : AuthorizationHandler<AffiliatedDatasetAuthorizationRequirement, AffiliatedDatasetResource>
	{
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly ILogger<AffiliatedDatasetAuthorizationHandler> _logger;

		public AffiliatedDatasetAuthorizationHandler(
			IPermissionPolicyService permissionPolicyService,
			ILogger<AffiliatedDatasetAuthorizationHandler> logger)
		{
			this._logger = logger;
			this._permissionPolicyService = permissionPolicyService;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AffiliatedDatasetAuthorizationRequirement requirement, AffiliatedDatasetResource contextResource)
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

			ISet<String> affiliatedPermissions = null;
			ISet<String> affiliatedRolePermissions = this._permissionPolicyService.PermissionsOfDataset(contextResource.AffiliatedRoles);
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
