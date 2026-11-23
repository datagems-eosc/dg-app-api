using Microsoft.AspNetCore.Authorization;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	/// <summary>
	/// Decides whether an <b>anonymous</b> caller (no user, or a user with no claims) satisfies a
	/// <see cref="PermissionAuthorizationRequirement"/>, by asking
	/// <see cref="IPermissionPolicyService.AllowAnonymous(String)"/> which permissions are open to anonymous access.
	/// For any authenticated caller it does nothing and leaves the decision to the other handlers.
	/// </summary>
	/// <remarks>
	/// <para><b>Who calls this</b></para>
	/// <para>
	/// Nothing in our code calls this class, and no middleware invokes it. It is only registered in DI. The only path
	/// that reaches it is:
	/// </para>
	/// <list type="number">
	///   <item><description>Code calls one of the <see cref="AuthorizationService"/> methods (<c>Authorize</c>, <c>AuthorizeForce</c>, <c>AuthorizeOrOwner</c>, ...).</description></item>
	///   <item><description><see cref="AuthorizationService"/> builds a <see cref="PermissionAuthorizationRequirement"/>, wraps it in an ad-hoc <see cref="Microsoft.AspNetCore.Authorization.AuthorizationPolicy"/> (no named policy is registered anywhere) and calls <c>AuthorizeAsync</c> on the <b>ASP.NET Core</b> <see cref="Microsoft.AspNetCore.Authorization.IAuthorizationService"/>.</description></item>
	///   <item><description>The framework runs <b>every</b> handler registered in DI as <see cref="Microsoft.AspNetCore.Authorization.IAuthorizationHandler"/>.</description></item>
	///   <item><description>This handler's inherited <c>HandleAsync</c> picks out the requirements matching its generic argument and calls <see cref="HandleRequirementAsync"/> once for each.</description></item>
	/// </list>
	/// <para>
	/// A bare <c>[Authorize]</c> attribute on an endpoint does <b>not</b> reach this handler. It only checks that
	/// the caller is authenticated, and its policy contains no <see cref="PermissionAuthorizationRequirement"/>.
	/// </para>
	/// <para><b>How the requirement is mapped to this handler</b></para>
	/// <para>
	/// Only by the generic argument in <c>AuthorizationHandler&lt;PermissionAuthorizationRequirement&gt;</c>.
	/// There is no explicit link between the requirement and its handlers. To find every handler of a requirement, search for
	/// <c>AuthorizationHandler&lt;PermissionAuthorizationRequirement&gt;</c>. The others include
	/// <see cref="PermissionRoleAuthorizationHandler"/> and <see cref="PermissionClaimAuthorizationHandler"/>.
	/// They all run for the same requirement, each judging it from a different angle (for example anonymous access, roles, claims).
	/// </para>
	/// <para><b>How the outcome works</b></para>
	/// <para>
	/// A handler cannot return "allowed" or "denied". It either calls <c>context.Succeed(requirement)</c> or does nothing
	/// (and returns <see cref="Task.CompletedTask"/>). This handler never calls <c>context.Fail()</c>.
	/// The requirement is satisfied if <b>at least one</b> handler succeeds it and no handler calls <c>context.Fail()</c>.
	/// Otherwise the check is denied, and <see cref="AuthorizationService"/> then returns <c>false</c> or throws, depending on the <c>Force</c> variant.
	/// </para>
	/// <para>
	/// <b>GOTCHA:</b> <see cref="PermissionAuthorizationRequirement.MatchAll"/> is evaluated per handler. This handler only counts
	/// the permissions that anonymous access can grant. Permissions granted through other handlers (roles, claims)
	/// are not added to its count.
	/// </para>
	/// <para>
	/// <b>Current configuration:</b> <c>AllowAnonymous</c> is <c>false</c> for every permission, so this handler currently never succeeds a requirement.
	/// </para>
	/// <para>
	/// Registered as a singleton, so it must not hold per-request state.
	/// </para>
	/// </remarks>
	public class PermissionAnonymousAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
	{
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly ILogger<PermissionAnonymousAuthorizationHandler> _logger;

		public PermissionAnonymousAuthorizationHandler(
			IPermissionPolicyService permissionPolicyService,
			ILogger<PermissionAnonymousAuthorizationHandler> logger)
		{
			this._logger = logger;
			this._permissionPolicyService = permissionPolicyService;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
		{
			Boolean isAnonymous = context.User == null || !context.User.Claims.Any();

			if (!isAnonymous)
			{
				this._logger.Trace("current user not anonymous");
				return Task.CompletedTask;
			}

			if (!requirement.RequiredPermissions.Any())
			{
				this._logger.Trace("no requirements specified");
				return Task.CompletedTask;
			}

			int hitCount = 0;
			foreach(String permission in requirement.RequiredPermissions)
			{
				Boolean hasPermission = this.HasPermission(permission);
				if (hasPermission) hitCount += 1;
			}

			this._logger.Trace("required {allcount} permissions, current principal has matched {hascount} and require all is set to: {matchall}", requirement.RequiredPermissions?.Count, hitCount, requirement.MatchAll);

			if((requirement.MatchAll && requirement.RequiredPermissions.Count == hitCount) ||
				!requirement.MatchAll && hitCount > 0) context.Succeed(requirement);

			return Task.CompletedTask;
		}

		private Boolean HasPermission(String permission)
		{
			return this._permissionPolicyService.AllowAnonymous(permission);
		}
	}
}
