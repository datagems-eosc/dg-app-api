
namespace DataGEMS.Gateway.App.Authorization
{
	/// <summary>
	/// Lookup of the mapping between permissions and the roles, context roles, claims and clients that grant them.
	/// The mapping comes from the <see cref="PermissionPolicyConfig"/> (the <c>Permissions</c> section of the configuration).
	/// </summary>
	/// <remarks>
	/// <para><b>Which method to use</b></para>
	/// <list type="table">
	///   <listheader><term>Question</term><description>Method family</description></listheader>
	///   <item><term>Which permissions do these <b>roles</b> give?</term>
	///     <description><c>PermissionsOf</c> (global roles) or <c>PermissionsOfContext</c> (context roles). Roles to permissions.</description></item>
	///   <item><term>Which <b>roles</b> grant this permission?</term>
	///     <description><c>RolesHaving</c> (global roles) or <c>ContextRolesHaving</c> (context roles). Permission to roles.</description></item>
	///   <item><term>Which <b>claim values</b> or <b>clients</b> grant this permission?</term>
	///     <description><c>ClaimsHaving</c> or <c>ClientsHaving</c>.</description></item>
	///   <item><term>Does this permission need <b>no roles at all</b>?</term>
	///     <description><c>AllowAnonymous</c> or <c>AllowAuthenticated</c>.</description></item>
	/// </list>
	/// <para><b>How the mapping is built</b></para>
	/// <para>
	/// The implementation reads <see cref="PermissionPolicyConfig.Policies"/> once, in its constructor, and builds both directions
	/// of the mapping (permission to roles and roles to permission) in memory. Nothing refreshes it afterwards, so a configuration
	/// change needs a restart. The permission names are matched as plain strings against the constants in <see cref="Permission"/>.
	/// </para>
	/// <para>
	/// <b>This service only looks things up. It does not decide anything.</b> Combining global roles with context roles, and
	/// applying the result for the current principal, happens in the authorization handlers. These are reached through
	/// <see cref="IAuthorizationService"/>, and the service is also a dependency of <see cref="IAuthorizationContentResolver"/>, which utilizes the service directly.
	/// </para>
	/// <para>
	/// <b>Unknown input:</b> an unknown permission or role is not an error. The lookups return an empty set (or <c>false</c> for the
	/// <c>Allow*</c> methods). This is also what happens when a name in the configuration and the matching constant in
	/// <c>Permission</c> differ, so a mismatch silently denies everyone.
	/// </para>
	/// <para>
	/// <b>GOTCHA:</b> the returned sets are the internal ones (or shared empty instances), not copies. Callers must treat them as read-only.
	/// </para>
	/// <para>
	/// <b>Current configuration:</b> claims are empty for every permission, and <c>AllowAnonymous</c> is <c>false</c> for every permission.
	/// Clients are mostly empty (only <c>EditDataset</c> lists one, <c>airflow</c>). <c>AllowAuthenticated</c> is mostly <c>false</c> (only
	/// <c>BrowseFieldsOfScienceVocabulary</c> and <c>BrowseLicenseVocabulary</c> set it to <c>true</c>).
	/// </para>
	/// </remarks>
	public interface IPermissionPolicyService
	{
		/// <summary>Gets all the permissions granted by the given global roles.</summary>
		/// <remarks>
		/// The result is the union across all given roles. Roles that are not configured are skipped.
		/// A <c>null</c> or empty <paramref name="roles"/> returns an empty set.
		/// </remarks>
		/// <param name="roles">Global roles (the realm roles from the token).</param>
		/// <returns>A set of permission names. Order is not guaranteed and there are no duplicates.</returns>
		ISet<String> PermissionsOf(IEnumerable<String> roles);

		/// <summary>Gets all the permissions granted by the given context roles.</summary>
		/// <remarks>
		/// The result is the union across all given roles. Roles that are not configured are skipped.
		/// A <c>null</c> or empty <paramref name="contextRoles"/> returns an empty set.
		/// </remarks>
		/// <param name="contextRoles">Context roles (the roles granted on a collection or a dataset).</param>
		/// <returns>A set of permission names. Order is not guaranteed and there are no duplicates.</returns>
		ISet<String> PermissionsOfContext(IEnumerable<String> contextRoles);

		/// <summary>Gets all the global roles that grant the given permission.</summary>
		/// <param name="permission">The permission to check.</param>
		/// <returns>A set of global roles. Empty if the permission is unknown or has no global roles.</returns>
		ISet<String> RolesHaving(String permission);

		/// <summary>Gets all the context roles that grant the given permission.</summary>
		/// <param name="permission">The permission to check.</param>
		/// <returns>A set of context roles. Empty if the permission is unknown or has no context roles.</returns>
		ISet<String> ContextRolesHaving(String permission);

		/// <summary>
		/// Gets the claim values that grant the given permission for the given claim.
		/// </summary>
		/// <remarks>
		/// Currently the claims are empty for every permission in the configuration, so this always returns an empty set.
		/// </remarks>
		/// <param name="claim">The claim whose values are wanted.</param>
		/// <param name="permission">The permission to check.</param>
		/// <returns>A set of claim values. Empty if the permission or the claim is unknown.</returns>
		ISet<String> ClaimsHaving(String claim, String permission);

		/// <summary>Gets the clients that are granted the given permission.</summary>
		/// <remarks>
		/// Mostly empty in the current configuration. Only <c>EditDataset</c> lists a client (<c>airflow</c>).
		/// </remarks>
		/// <param name="permission">The permission to check.</param>
		/// <returns>A set of clients. Empty if the permission is unknown or has no clients.</returns>
		ISet<String> ClientsHaving(String permission);

		/// <summary>Does the permission allow anonymous access?</summary>
		/// <remarks>
		/// Currently <c>false</c> for every permission in the configuration.
		/// </remarks>
		/// <param name="permission">The permission to check.</param>
		/// <returns><c>true</c> if anonymous access is allowed. <c>false</c> otherwise, including for an unknown permission.</returns>
		Boolean AllowAnonymous(String permission);

		/// <summary>
		/// Does the permission allow any authenticated principal? No role is required, but authentication is.
		/// </summary>
		/// <remarks>
		/// Mostly <c>false</c> in the current configuration. Only <c>BrowseFieldsOfScienceVocabulary</c> and
		/// <c>BrowseLicenseVocabulary</c> set it to <c>true</c>.
		/// </remarks>
		/// <param name="permission">The permission to check.</param>
		/// <returns><c>true</c> if any authenticated principal is allowed. <c>false</c> otherwise, including for an unknown permission.</returns>
		Boolean AllowAuthenticated(String permission);
	}
}
