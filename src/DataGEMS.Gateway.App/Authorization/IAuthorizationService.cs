namespace DataGEMS.Gateway.App.Authorization
{
	/// <summary>
	/// Application-level authorization checks for the current principal.
	/// This wraps ASP.NET Core's <see cref="Microsoft.AspNetCore.Authorization.IAuthorizationService"/>
	/// and is not the same type.
	/// </summary>
	/// <remarks>
	/// <para><b>Which kind of check to use</b></para>
	/// <list type="table">
	///   <listheader><term>Question</term><description>Method family (typical data)</description></listheader>
	///   <item><term>Does the user (or their group) hold a <b>role in a context</b> (e.g. a collection, a dataset, or content that belongs to one)?</term>
	///     <description><c>AuthorizeOrAffiliatedContext</c> (or <c>AuthorizeAffiliatedContext</c> when global permissions must not bypass context checks). Often paired with downstream query or service scoping when a specific target ID is not yet known.</description></item>
	///   <item><term>Does this thing <b>belong to this user</b>?</term>
	///     <description><c>AuthorizeOwner</c>. One-to-one data such as user settings, user favourites, ad hoc queries, or a user record itself.</description></item>
	///   <item><term>Was the user (or their group) <b>granted a role on this collection/dataset</b>?</term>
	///     <description><c>AuthorizeOrAffiliatedContext</c> / <c>AuthorizeAffiliatedContext</c>. Many-to-many shared data such as collections and datasets (and entities tied to them, such as dataset-scoped workflow processes).</description></item>
	///   <item><term>Global permission <b>or</b> owner / grant?</term>
	///     <description><c>AuthorizeOr...</c>. E.g. a conversation or workflow process where global roles, ownership, or context roles can grant access.</description></item>
	/// </list>
	/// <para><b>How to read the method names</b></para>
	/// <list type="bullet">
	///   <item><term>(no suffix)</term><description>Returns <c>true</c> or <c>false</c>. Never throws on denial.</description></item>
	///   <item><term>...Force</term><description>Returns <c>true</c> if allowed and <b>throws <see cref="DGForbiddenException"/></b> if denied. It can never return <c>false</c>.</description></item>
	///   <item><term>...Owner</term><description>Checks that the principal's subject ID is in <c>OwnedResource.UserIds</c>. No permissions are involved. Despite the name, <c>UserIds</c> holds <b>subject IDs</b> from the token.</description></item>
	///   <item><term>...AffiliatedContext</term><description>Checks permissions within the scope of an <see cref="AffiliatedContextResource"/> (the roles and permissions the principal holds in some context, resolved by the caller beforehand). In practice, <c>AuthorizeOrAffiliatedContext*</c> is used far more than standalone <c>AuthorizeAffiliatedContext*</c>, and <c>Authorize(Object, String[])</c> is largely unused in favor of the dedicated methods.</description></item>
	///   <item><term>AuthorizeOr...</term><description>Passes if the principal has the permissions <b>globally</b>; otherwise falls back to the owner or affiliated-context check.</description></item>
	/// </list>
	/// <para><b>How the decision is made</b></para>
	/// <para>
	/// Every method builds a requirement, wraps it in an ad-hoc policy and calls <c>AuthorizeAsync</c> on the
	/// ASP.NET Core authorization service. That call is the <b>only</b> thing that runs the authorization handlers
	/// (e.g. <see cref="PermissionRoleAuthorizationHandler"/>, <see cref="OwnedResourceAuthorizationHandler"/>,
	/// <see cref="AffiliatedContextAuthorizationHandler"/>). Nothing references them directly and no middleware calls them.
	/// Each handler is matched to its requirement only by the generic type argument in its declaration.
	/// A bare <c>[Authorize]</c> on an endpoint only checks that the caller is logged in, so permission checks
	/// happen only if code calls a method of this interface.
	/// </para>
	/// <para>
	/// <b>Censors vs. downstream queries, services, and builders:</b> When a global permission check passes, the
	/// authorization decision is complete at the call site. When <c>AuthorizeOwner*</c> runs with a known owner
	/// <c>userId</c> (mapped to a subject ID), the ownership check is also complete at the call site. By contrast,
	/// when <c>AuthorizeOrAffiliatedContext*</c> is called in a pre-entity censor with unscoped roles from
	/// <c>ContextRolesOf()</c> (before a specific entity or target ID is loaded), a context-only pass only shows
	/// the user holds a qualifying context role in <i>at least one</i> context—downstream queries or services still
	/// enforce per-resource/dataset/collection access (and handle creation-time checks where <c>isNewItem</c> bypasses
	/// censor enforcement).
	/// </para>
	/// <para>
	/// <b>GOTCHA:</b> every permission check uses OR semantics (<c>matchAll: false</c>): any one of the listed
	/// permissions is enough. This is aligned with the UI logic that shows and hides sections.
	/// </para>
	/// </remarks>
	public interface IAuthorizationService
	{
		/// <summary>Does the principal hold at least one of the permissions? Global check, no resource.</summary>
		/// <param name="permissions">Permission names. Any one is sufficient.</param>
		/// <returns><c>true</c> if allowed, otherwise <c>false</c>. Never throws on denial.</returns>
		Task<Boolean> Authorize(params String[] permissions);

		/// <summary>
		/// Does the principal hold at least one of the permissions with respect to <paramref name="resource"/>?
		/// </summary>
		/// <remarks>
		/// <para>
		/// Largely unused in practice in favor of the dedicated <c>AuthorizeOwner*</c> and <c>AuthorizeOrAffiliatedContext*</c> methods.
		/// </para>
		/// </remarks>
		/// <param name="resource">The resource handed to the authorization handlers. If <c>null</c>, this behaves like a global check.</param>
		/// <param name="permissions">Permission names. Any one is sufficient.</param>
		/// <returns><c>true</c> if allowed, otherwise <c>false</c>. Never throws on denial.</returns>
		Task<Boolean> Authorize(Object resource, params String[] permissions);

		/// <summary>Same as <see cref="Authorize(String[])"/>, but it throws if denied.</summary>
		/// <param name="permissions">Permission names. Any one is sufficient.</param>
		/// <returns>Always <c>true</c> when it returns. Denial is signalled by the exception.</returns>
		/// <exception cref="DGForbiddenException"></exception>
		Task<Boolean> AuthorizeForce(params String[] permissions);

		/// <summary>
		/// Same as <see cref="Authorize(Object,String[])"/>, but it throws if denied.
		/// Largely unused in practice in favor of the dedicated <c>*Force</c> methods.
		/// </summary>
		/// <param name="resource">The resource handed to the authorization handlers. If <c>null</c>, this behaves like a global check.</param>
		/// <param name="permissions">Permission names. Any one is sufficient.</param>
		/// <returns>Always <c>true</c> when it returns. Denial is signalled by the exception.</returns>
		/// <exception cref="DGForbiddenException"></exception>
		Task<Boolean> AuthorizeForce(Object resource, params String[] permissions);

		/// <summary>
		/// Does the resource belong to the current principal? True if the principal's subject ID is listed in
		/// <see cref="OwnedResource.UserIds"/>. No permissions are involved.
		/// </summary>
		/// <remarks>
		/// Use this for data that belongs to a specific user (one-to-one), such as user settings, user favourites,
		/// ad hoc queries, or a user record itself.
		/// <para>
		/// Despite its name, <see cref="OwnedResource.UserIds"/> holds <b>subject IDs</b> (the user id from the token).
		/// </para>
		/// <para>
		/// Not for shared content such as collections and datasets. Access to those is decided by context grants,
		/// see <see cref="AuthorizeAffiliatedContext"/>. To allow the owner <b>or</b> anyone holding a global
		/// permission (e.g. conversations, where an admin may also see them), use <see cref="AuthorizeOrOwner"/>.
		/// </para>
		/// </remarks>
		/// <param name="resource">
		/// Describes who owns the thing being checked. <see cref="OwnedResource.UserIds"/> holds the owners' subject IDs.
		/// <see cref="OwnedResource.ResourceType"/> is informational; the ownership handler does not read it.
		/// </param>
		/// <returns>
		/// <c>true</c> if the principal's subject ID is in <see cref="OwnedResource.UserIds"/>, otherwise <c>false</c>.
		/// Never throws on denial; see <see cref="AuthorizeOwnerForce"/> for the throwing variant.
		/// </returns>
		Task<Boolean> AuthorizeOwner(OwnedResource resource);

		/// <summary>Same as <see cref="AuthorizeOwner(OwnedResource)"/>, but it throws if denied.</summary>
		/// <param name="resource">
		/// Describes who owns the thing being checked. <see cref="OwnedResource.UserIds"/> holds the owners' subject IDs.
		/// <see cref="OwnedResource.ResourceType"/> is informational; the ownership handler does not read it.
		/// </param>
		/// <returns>Always <c>true</c> when it returns. Denial is signalled by the exception.</returns>
		/// <exception cref="DGForbiddenException"></exception>
		Task<Boolean> AuthorizeOwnerForce(OwnedResource resource);

		/// <summary>
		/// Does the principal hold at least one of the permissions <b>within a specific context</b>, where the context is
		/// represented by the roles and permissions the principal has in it (<paramref name="contextResource"/>)?
		/// Used for shared (many-to-many) content, where access comes from roles granted to users or groups on a
		/// context (for example a collection or a dataset), not from the token and not from ownership.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>This method does not look up grants.</b> <paramref name="contextResource"/> only carries
		/// <see cref="AffiliatedContextResource.AffiliatedRoles"/> and <see cref="AffiliatedContextResource.AffiliatedPermissions"/>.
		/// It has no id or type of the context, so it is only meaningful if the caller built it for the context being checked.
		/// </para>
		/// <para>
		/// Typical flow:
		/// <list type="number">
		///   <item><description>Load the principal's roles for that context, typically through <see cref="IAuthorizationContentResolver"/>
		///   (e.g. <c>ContextRolesForCollectionOfUser</c> or <c>EffectiveContextRolesForDatasetOfUser</c>). The resolver reads the context grants
		///   stored in Keycloak through <c>IAAIService</c>.</description></item>
		///   <item><description>Put the result in an <see cref="AffiliatedContextResource"/>.</description></item>
		///   <item><description>Call this method. It never calls Keycloak itself.</description></item>
		/// </list>
		/// </para>
		/// <para>
		/// Global permissions (the realm roles in the token) are <b>not</b> considered here. To allow either a global permission
		/// or a context grant, use <see cref="AuthorizeOrAffiliatedContext"/>.
		/// </para>
		/// <para>
		/// The handler (<see cref="AffiliatedContextAuthorizationHandler"/>) turns <c>AffiliatedRoles</c> into permissions through
		/// <c>IPermissionPolicyService.PermissionsOfContext</c>, adds any <c>AffiliatedPermissions</c>, and succeeds if at least one
		/// required permission is in that set. A <c>null</c> <paramref name="contextResource"/> is not evaluated and is denied,
		/// and so is a principal with no claims.
		/// </para>
		/// </remarks>
		/// <param name="contextResource">The principal's roles and/or permissions within the context being checked, loaded beforehand.</param>
		/// <param name="permissions">Permission names. Any one is sufficient.</param>
		/// <returns><c>true</c> if allowed, otherwise <c>false</c>. Never throws on denial.</returns>
		Task<Boolean> AuthorizeAffiliatedContext(AffiliatedContextResource contextResource, params String[] permissions);

		/// <summary>Same as <see cref="AuthorizeAffiliatedContext"/>, but throws if denied.</summary>
		/// <remarks>
		/// The same precondition applies: the caller must load the context grants first (see <see cref="AuthorizeAffiliatedContext"/>).
		/// A <c>null</c> <paramref name="contextResource"/> is denied, which here means it throws.
		/// </remarks>
		/// <param name="contextResource">The principal's roles and/or permissions within the context being checked, loaded beforehand.</param>
		/// <param name="permissions">Permission names. Any one is sufficient.</param>
		/// <returns>Always <c>true</c> when it returns. Denial is signalled by the exception.</returns>
		/// <exception cref="DGForbiddenException"></exception>
		Task<Boolean> AuthorizeAffiliatedContextForce(AffiliatedContextResource contextResource, params String[] permissions);

		/// <summary>
		/// Passes if <b>either</b> <see cref="Authorize(String[])"/> (the principal holds at least one of the permissions
		/// globally) <b>or</b> <see cref="AuthorizeOwner"/> (the principal owns <paramref name="resource"/>) succeeds.
		/// </summary>
		/// <remarks>
		/// The global permission check runs first. The ownership check only runs if it fails.
		/// A <c>null</c> <paramref name="resource"/> skips the ownership check, so only the global permission can pass.
		/// <para>
		/// Typical use: data that belongs to a user but that privileged users may also see (e.g. conversations, user records).
		/// </para>
		/// <para>
		/// Callers usually only have the gateway user id of the row's owner, so they must translate it first, e.g. with
		/// <c>IAuthorizationContentResolver.SubjectIdOfUserId</c>. Passing a gateway user id here never matches.
		/// </para>
		/// </remarks>
		/// <param name="resource">Describes who owns the thing being checked (see <see cref="AuthorizeOwner"/>).</param>
		/// <param name="permissions">Global permission names. Any one is sufficient.</param>
		/// <returns><c>true</c> if either check passes, otherwise <c>false</c>. Never throws on denial.</returns>
		Task<Boolean> AuthorizeOrOwner(OwnedResource resource, params String[] permissions);

		/// <summary>
		/// Same as <see cref="AuthorizeOrOwner"/>, but throws if neither check passes.
		/// </summary>
		/// <remarks>
		/// The global permission check is non-throwing, so a failure there falls through to the ownership check.
		/// Only the final outcome throws. If <paramref name="resource"/> is <c>null</c>, this is equivalent to
		/// <see cref="AuthorizeForce(String[])"/>.
		/// </remarks>
		/// <param name="resource">Describes who owns the thing being checked (see <see cref="AuthorizeOwner"/>).</param>
		/// <param name="permissions">Global permission names. Any one is sufficient.</param>
		/// <returns>Always <c>true</c> when it returns. Denial is signalled by the exception.</returns>
		/// <exception cref="DGForbiddenException">The principal lacks the permissions and does not own the resource.</exception>
		Task<Boolean> AuthorizeOrOwnerForce(OwnedResource resource, params String[] permissions);

		/// <summary>
		/// Passes if <b>either</b> <see cref="Authorize(String[])"/> (the principal holds at least one of the permissions
		/// globally) <b>or</b> <see cref="AuthorizeAffiliatedContext"/> (the principal holds at least one of the permissions
		/// within the context described by <paramref name="contextResource"/>) succeeds.
		/// </summary>
		/// <remarks>
		/// The global permission check runs first. The context check only runs if it fails.
		/// A <c>null</c> <paramref name="contextResource"/> skips the context check, so only the global permission can pass.
		/// <para>
		/// As with <see cref="AuthorizeAffiliatedContext"/>, this method does not look up grants. The caller must load the
		/// principal's roles and permissions for the context beforehand, typically through <see cref="IAuthorizationContentResolver"/>.
		/// </para>
		/// <para>
		/// Typical use: shared content where privileged users with a global permission may access everything, while other
		/// users may access only the contexts they were granted a role on.
		/// </para>
		/// </remarks>
		/// <param name="contextResource">The principal's roles and/or permissions within the context being checked, loaded beforehand (see <see cref="AuthorizeAffiliatedContext"/>).</param>
		/// <param name="permissions">Permission names. Any one is sufficient, both for the global check and for the context check.</param>
		/// <returns><c>true</c> if either check passes, otherwise <c>false</c>. Never throws on denial.</returns>
		Task<Boolean> AuthorizeOrAffiliatedContext(AffiliatedContextResource contextResource, params String[] permissions);

		/// <summary>
		/// Same as <see cref="AuthorizeOrAffiliatedContext"/>, but throws if neither check passes.
		/// </summary>
		/// <remarks>
		/// The global permission check is non-throwing, so a failure there falls through to the context check.
		/// Only the final outcome throws. If <paramref name="contextResource"/> is <c>null</c>, this is equivalent to
		/// <see cref="AuthorizeForce(String[])"/>.
		/// </remarks>
		/// <param name="contextResource">The principal's roles and/or permissions within the context being checked, loaded beforehand (see <see cref="AuthorizeAffiliatedContext"/>).</param>
		/// <param name="permissions">Permission names. Any one is sufficient, both for the global check and for the context check.</param>
		/// <returns>Always <c>true</c> when it returns. Denial is signalled by the exception.</returns>
		/// <exception cref="DGForbiddenException"></exception>
		Task<Boolean> AuthorizeOrAffiliatedContextForce(AffiliatedContextResource contextResource, params String[] permissions);
	}
}