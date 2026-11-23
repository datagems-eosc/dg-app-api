using Microsoft.AspNetCore.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	/// <summary>
	/// A requirement that says "the principal must hold these permissions".
	/// It is only a description of what is needed. It does not decide anything itself.
	/// The registered authorization handlers evaluate it and decide whether it is satisfied.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Instances are created by <see cref="AuthorizationService"/> and passed to the framework
	/// authorization service as part of an <see cref="Microsoft.AspNetCore.Authorization.AuthorizationPolicy"/>.
	/// </para>
	/// <para>
	/// Endpoints are protected by <c>[Authorize]</c>, which only guarantees the caller is authenticated.
	/// Fine-grained permission checks are not applied by the framework automatically; they happen
	/// only when code calls one of the <see cref="App.Authorization.IAuthorizationService"/> methods.
	/// </para>
	/// <para>
	/// <b>GOTCHA:</b> <see cref="MatchAll"/> is evaluated by each handler separately. To satisfy it, all the permissions
	/// must be matched by the same handler (all by role, or all by client, or all anonymous, etc.).
	/// <see cref="AuthorizationService"/> currently always sets it to <c>false</c>, so any one permission is enough.
	/// </para>
	/// </remarks>
	public class PermissionAuthorizationRequirement : IAuthorizationRequirement
	{
		public List<String> RequiredPermissions { get; private set; }
		public Boolean MatchAll { get; private set; }

		public PermissionAuthorizationRequirement(List<String> requiredPermissions, Boolean matchAll = false)
		{
			this.RequiredPermissions = requiredPermissions;
			this.MatchAll = matchAll;
		}
	}
}
