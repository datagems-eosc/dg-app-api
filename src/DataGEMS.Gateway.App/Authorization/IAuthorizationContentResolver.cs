using DataGEMS.Gateway.App.Common.Auth;

namespace DataGEMS.Gateway.App.Authorization
{
	/// <summary>
	/// Identity and context-grant lookups. Some methods work on the current principal, others on an explicit user (by subject ID)
	/// or user group, and others on the grants of a dataset regardless of who holds them. This is the interface that services,
	/// queries and controllers use almost exclusively, and it sits on top of <see cref="IAuthorizationService"/>,
	/// <see cref="IPermissionPolicyService"/> and <c>IAAIService</c>.
	/// </summary>
	/// <remarks>
	/// <para><b>Which method to use</b></para>
	/// <list type="table">
	///   <listheader><term>Question</term><description>Method family</description></listheader>
	///   <item><term>Who is the current user?</term>
	///     <description><c>CurrentUser</c> (subject ID from the token, no database), <c>CurrentUserId</c> (gateway user ID) or <c>SubjectIdOfCurrentUser</c> (subject ID, verified against the gateway database).</description></item>
	///   <item><term>I have the ID of another user and need their subject ID.</term>
	///     <description><c>SubjectIdOfUserId</c> (gateway user ID) or <c>SubjectIdOfUserIdentifier</c> (either kind, as a string).</description></item>
	///   <item><term>Does the current user hold a <b>global</b> permission?</term>
	///     <description><c>HasPermission</c>.</description></item>
	///   <item><term>Which <b>context roles</b> does a user or group hold on specific collections or datasets?</term>
	///     <description><c>ContextRolesForCollectionOf*</c> and <c>EffectiveContextRolesForDatasetOf*</c> (current user, explicit subject ID, or user group). The result is used to build an <see cref="AffiliatedContextResource"/>.</description></item>
	///   <item><term>Which collections or datasets does the current user hold a given <b>permission</b> on?</term>
	///     <description><c>ContextAffiliatedCollections</c> and <c>EffectiveContextAffiliatedDatasets</c>. Typically used to scope a query.</description></item>
	///   <item><term>Who has grants on these datasets, regardless of user?</term>
	///     <description><c>RetrieveAllDatasetContextGrants</c> or <c>RetrieveEffectiveDatasetContextGrants</c>.</description></item>
	/// </list>
	/// <para><b>User ID vs subject ID</b></para>
	/// <para>
	/// The user ID is the ID of the user in the gateway database. The subject ID is the ID of the user in Keycloak, and it is
	/// what the token carries and what <c>IAAIService</c> expects. These are not the same. The context role lookups take a
	/// <b>subject ID</b>, so a user ID must be translated first with <c>SubjectIdOfUserId</c> or <c>SubjectIdOfUserIdentifier</c>.
	/// </para>
	/// <para><b>How to read the method names</b></para>
	/// <list type="bullet">
	///   <item><term>...OfCurrentUser / ...Of (no ID)</term><description>Uses the principal's subject ID, resolved through <see cref="SubjectIdOfCurrentUser"/>.</description></item>
	///   <item><term>...OfUser(subjectId, ...)</term><description>Same lookup for an explicit subject ID instead of the current principal.</description></item>
	///   <item><term>...OfUserGroup(groupId, ...)</term><description>Same lookup for a user group instead of a user.</description></item>
	///   <item><term>Single vs. multiple IDs</term><description>The single-ID overloads call the multiple-ID overloads. Prefer the multiple-ID ones when checking many items, since each call fetches the grants again (through the AAI service, which caches them; the gateway database is still queried every time to resolve the current user).</description></item>
	///   <item><term>Effective...</term><description>Dataset counterparts of the collection methods. See the remark below.</description></item>
	/// </list>
	/// <para><b>What "Effective" means</b></para>
	/// <para>
	/// The <c>Effective...</c> dataset methods do the same thing as their collection counterparts, filtering the grants by
	/// <c>TargetKind.Dataset</c> instead of <c>TargetKind.Collection</c>. Only grants made directly on the dataset are read.
	/// Roles on the collections that contain the dataset are not merged in.
	/// </para>
	/// <para><b>Roles granted through groups</b></para>
	/// <para>
	/// The user lookups (the <c>...OfUser</c>, <c>...OfCurrentUser</c> and <c>ContextAffiliated*</c> methods, and <c>ContextRolesOf</c>) include the roles
	/// the user holds through the user groups they belong to, for collections and datasets alike. The <c>...OfUserGroup</c> methods only return the grants of that one group.
	/// </para>
	/// <para><b>Shape of the results</b></para>
	/// <list type="bullet">
	///   <item><description>Methods returning role sets or ID lists return an empty collection, never <c>null</c>, when the user is unknown or has no matching grants.</description></item>
	///   <item><description>Dictionaries only contain the IDs on which the user or group being looked up holds at least one role. A missing key means no roles.</description></item>
	///   <item><description>The ID lookups (<c>CurrentUserId</c>, <c>SubjectIdOf*</c>) return <c>null</c> when nothing is found.</description></item>
	///   <item><description>The <c>Retrieve*DatasetContextGrants</c> methods return one <see cref="DatasetContextGrants"/> per requested dataset, with empty <c>Users</c> and <c>UserGroups</c> if it has no grants.</description></item>
	/// </list>
	/// </remarks>
	public interface IAuthorizationContentResolver
	{
		/// <summary>
		/// Checks whether there is a current principal.
		/// </summary>
		/// <remarks>
		/// Rarely used, since the <c>[Authorize]</c> attribute already rejects unauthenticated callers.
		/// </remarks>
		/// <returns><c>true</c> if a principal exists, otherwise <c>false</c>.</returns>
		Boolean HasAuthenticated();

		/// <summary>
		/// Gets the subject ID of the current user from the claims principal built during authentication.
		/// No database or Keycloak lookup is involved.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The user ID is the ID in the gateway database, the subject ID is the ID in Keycloak. These are not the same.
		/// Use <see cref="SubjectIdOfCurrentUser"/> if the user must also exist in the gateway database.
		/// </para>
		/// </remarks>
		/// <returns>The subject ID taken from the token.</returns>
		String CurrentUser();

		/// <summary>
		/// Gets the gateway user ID of the current user. It reads the subject ID with <see cref="CurrentUser"/> and looks up the matching user in the database.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The user ID is the ID in the gateway database, the subject ID is the ID in Keycloak. These are not the same.
		/// </para>
		/// </remarks>
		/// <returns>The user ID, or <c>null</c> if there is no subject ID or no matching user in the database.</returns>
		Task<Guid?> CurrentUserId();

		/// <summary>
		/// Gets the subject ID of the current user, like <see cref="CurrentUser"/>, but it goes through the database
		/// (<see cref="CurrentUserId"/> then <see cref="SubjectIdOfUserId"/>). This also confirms that the link between the subject ID and the
		/// user ID exists in the gateway database.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is the method the context role lookups use to identify the current user.
		/// </para>
		/// </remarks>
		/// <returns>The subject ID, or <c>null</c> if the user is not found in the gateway database.</returns>
		Task<String> SubjectIdOfCurrentUser();

		/// <summary>
		/// Gets the subject ID of the user with the given gateway user ID. Same as <see cref="SubjectIdOfCurrentUser"/>,
		/// but for any user, so it does not use the claims principal.
		/// </summary>
		/// <param name="userId">The gateway user ID of the user.</param>
		/// <returns>The subject ID stored for that user, or <c>null</c> if <paramref name="userId"/> is <c>null</c> or no such user exists.</returns>
		Task<String> SubjectIdOfUserId(Guid? userId);

		/// <summary>
		/// Gets the subject ID of a user from an identifier that may be either a gateway user ID or a subject ID.
		/// Like <see cref="SubjectIdOfUserId"/>, it does not use the claims principal.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the identifier parses as a <see cref="Guid"/>, it is tried as a user ID first. If that finds nothing, or it is not a <see cref="Guid"/>,
		/// it is looked up as a subject ID. The result is always read from the database, so a subject ID that is not stored in the gateway database returns <c>null</c>.
		/// </para>
		/// </remarks>
		/// <param name="userIdentifier">Either the user ID or the subject ID, as a string.</param>
		/// <returns>The subject ID, or <c>null</c> if the identifier is empty or matches no user.</returns>
		Task<String> SubjectIdOfUserIdentifier(String userIdentifier);

		/// <summary>
		/// Same as <see cref="SubjectIdOfCurrentUser"/>, but the subject ID is returned as a <see cref="Guid"/>.
		/// </summary>
		/// <returns>The subject ID as a <see cref="Guid"/>, or <c>null</c> if the subject ID was not found.</returns>
		/// <exception cref="DGApplicationException">The subject ID was found but is not a valid <see cref="Guid"/>.</exception>
		Task<Guid?> SubjectIdOfCurrentUserAsGuid();

		/// <summary>
		/// Does the current principal hold at least one of the permissions globally? Wraps <see cref="IAuthorizationService.Authorize(String[])"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Only permissions granted by global roles are checked. Ownership and context roles are not considered.
		/// </para>
		/// </remarks>
		/// <param name="permissions">Permission names. Any one is sufficient.</param>
		/// <returns><c>true</c> if allowed, otherwise <c>false</c>.</returns>
		Task<Boolean> HasPermission(params String[] permissions);

		/// <summary>
		/// Gets the permissions granted by the given context roles. Wraps <see cref="IPermissionPolicyService.PermissionsOfContext"/>.
		/// </summary>
		/// <param name="roles">The context roles.</param>
		/// <returns>A set of permissions. It is the union across the roles, with no duplicates and no guaranteed order. Empty if <paramref name="roles"/> is <c>null</c> or empty.</returns>
		ISet<String> PermissionsOfContextRoles(IEnumerable<String> roles);

		/// <summary>
		/// Gets the context roles the current user holds on one collection, including roles granted through the user's groups.
		/// </summary>
		/// <param name="collectionId">The collection ID.</param>
		/// <returns>A set of context roles. Empty if the user is unknown or holds no role on the collection.</returns>
		Task<HashSet<String>> ContextRolesForCollectionOfUser(Guid collectionId);

		/// <summary>
		/// Gets the context roles the given user holds on one collection, including roles granted through the user's groups.
		/// </summary>
		/// <param name="subjectId">The subject ID of the user.</param>
		/// <param name="collectionId">The collection ID.</param>
		/// <returns>A set of context roles. Empty if the user holds no role on the collection.</returns>
		Task<HashSet<String>> ContextRolesForCollectionOfUser(String subjectId, Guid collectionId);

		/// <summary>
		/// Gets the context roles the current user holds on each of the given collections, including roles granted through the user's groups.
		/// </summary>
		/// <param name="collectionIds">The collection IDs.</param>
		/// <returns>A dictionary from collection ID to context roles. Collections with no role have no entry. Empty if the user is unknown or <paramref name="collectionIds"/> is <c>null</c> or empty.</returns>
		Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollectionOfUser(IEnumerable<Guid> collectionIds);

		/// <summary>
		/// Gets the context roles the given user holds on each of the given collections, including roles granted through the user's groups.
		/// </summary>
		/// <remarks>
		/// This is the method the other <c>ContextRolesForCollectionOfUser</c> overloads end up calling. It reads all the context grants of the user
		/// through <c>IAAIService</c> and keeps the collection ones that match <paramref name="collectionIds"/>.
		/// </remarks>
		/// <param name="subjectId">The subject ID of the user.</param>
		/// <param name="collectionIds">The collection IDs.</param>
		/// <returns>A dictionary from collection ID to context roles. Collections with no role have no entry. Empty if <paramref name="collectionIds"/> is <c>null</c> or empty.</returns>
		Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollectionOfUser(String subjectId, IEnumerable<Guid> collectionIds);

		/// <summary>
		/// Gets the context roles a user group holds on one collection. Same as <see cref="ContextRolesForCollectionOfUser(String, Guid)"/>, but for a group.
		/// </summary>
		/// <remarks>
		/// Not really used, since the multiple-ID overload is used instead.
		/// </remarks>
		/// <param name="groupId">The ID of the user group.</param>
		/// <param name="collectionId">The collection ID.</param>
		/// <returns>A set of context roles. Empty if the group holds no role on the collection.</returns>
		Task<HashSet<String>> ContextRolesForCollectionOfUserGroup(String groupId, Guid collectionId);

		/// <summary>
		/// Gets the context roles a user group holds on each of the given collections. Same as <see cref="ContextRolesForCollectionOfUser(String, IEnumerable{Guid})"/>, but for a group.
		/// </summary>
		/// <param name="groupId">The ID of the user group.</param>
		/// <param name="collectionIds">The collection IDs.</param>
		/// <returns>A dictionary from collection ID to context roles. Collections with no role have no entry. Empty if <paramref name="collectionIds"/> is <c>null</c> or empty.</returns>
		Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollectionOfUserGroup(String groupId, IEnumerable<Guid> collectionIds);

		/// <summary>
		/// Gets the context roles the current user holds on one dataset, including roles granted through the user's groups.
		/// Dataset counterpart of <see cref="ContextRolesForCollectionOfUser(Guid)"/>.
		/// </summary>
		/// <param name="datasetId">The dataset ID.</param>
		/// <returns>A set of context roles. Empty if the user is unknown or holds no role on the dataset.</returns>
		Task<HashSet<String>> EffectiveContextRolesForDatasetOfUser(Guid datasetId);

		/// <summary>
		/// Gets the context roles the current user holds on each of the given datasets, including roles granted through the user's groups.
		/// Dataset counterpart of <see cref="ContextRolesForCollectionOfUser(IEnumerable{Guid})"/>.
		/// </summary>
		/// <param name="datasetIds">The dataset IDs.</param>
		/// <returns>A dictionary from dataset ID to context roles. Datasets with no role have no entry. Empty if the user is unknown or <paramref name="datasetIds"/> is <c>null</c> or empty.</returns>
		Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDatasetOfUser(IEnumerable<Guid> datasetIds);

		/// <summary>
		/// Gets the context roles the given user holds on each of the given datasets, including roles granted through the user's groups.
		/// Dataset counterpart of <see cref="ContextRolesForCollectionOfUser(String, IEnumerable{Guid})"/>.
		/// </summary>
		/// <param name="subjectId">The subject ID of the user.</param>
		/// <param name="datasetIds">The dataset IDs.</param>
		/// <returns>A dictionary from dataset ID to context roles. Datasets with no role have no entry. Empty if <paramref name="datasetIds"/> is <c>null</c> or empty.</returns>
		Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDatasetOfUser(String subjectId, IEnumerable<Guid> datasetIds);

		/// <summary>
		/// Gets the context roles a user group holds on each of the given datasets. Dataset counterpart of <see cref="ContextRolesForCollectionOfUserGroup(String, IEnumerable{Guid})"/>.
		/// </summary>
		/// <param name="groupId">The ID of the user group.</param>
		/// <param name="datasetIds">The dataset IDs.</param>
		/// <returns>A dictionary from dataset ID to context roles. Datasets with no role have no entry. Empty if <paramref name="datasetIds"/> is <c>null</c> or empty.</returns>
		Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDatasetOfUserGroup(String groupId, IEnumerable<Guid> datasetIds);

		/// <summary>
		/// Gets all the distinct context roles the current user holds, across every collection and dataset, including roles granted through the user's groups.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The result is not scoped to any target, so it only tells which roles the user holds somewhere. The grant's target
		/// is dropped. Per-target access must still be enforced downstream (see the censor remark in <see cref="IAuthorizationService"/>).
		/// </para>
		/// <para>
		/// Global roles are not returned.
		/// </para>
		/// </remarks>
		/// <returns>A list of context roles with no duplicates. Empty if the user is unknown.</returns>
		Task<List<String>> ContextRolesOf();

		/// <summary>
		/// Gets the IDs of the collections on which the current user holds a context role that grants the permission.
		/// </summary>
		/// <remarks>
		/// Typically used to scope a query to the collections the user may access. Roles granted through the user's groups count.
		/// </remarks>
		/// <param name="permission">The permission to check.</param>
		/// <returns>A list of collection IDs with no duplicates. Empty if the user is unknown or no context role grants the permission.</returns>
		Task<List<Guid>> ContextAffiliatedCollections(String permission);

		/// <summary>
		/// Gets the IDs of the datasets on which the current user holds a context role that grants the permission.
		/// Dataset counterpart of <see cref="ContextAffiliatedCollections"/>.
		/// </summary>
		/// <param name="permission">The permission to check.</param>
		/// <returns>A list of dataset IDs with no duplicates. Empty if the user is unknown or no context role grants the permission.</returns>
		Task<List<Guid>> EffectiveContextAffiliatedDatasets(String permission);

		/// <summary>
		/// Gets the context grants of every user and group on each of the given datasets.
		/// </summary>
		/// <remarks>
		/// <para>
		/// No permission check happens here, so the caller must authorize first.
		/// </para>
		/// <para>
		/// For users, the principal ID in the result is the subject ID. For groups, it is the Keycloak group ID.
		/// </para>
		/// </remarks>
		/// <param name="datasetIds">The dataset IDs.</param>
		/// <returns>One <see cref="DatasetContextGrants"/> per requested dataset, with empty <c>Users</c> and <c>UserGroups</c> if it has no grants. Empty if <paramref name="datasetIds"/> is <c>null</c> or empty.</returns>
		Task<List<DatasetContextGrants>> RetrieveAllDatasetContextGrants(IEnumerable<Guid> datasetIds);

		/// <summary>
		/// Gets the context grants of every user and group, but only for those of the given datasets on which the current user
		/// holds a context role that grants the permission.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The datasets are filtered with <see cref="EffectiveContextRolesForDatasetOfUser(IEnumerable{Guid})"/>, then the grants of the remaining ones
		/// are read with <see cref="RetrieveAllDatasetContextGrants"/>.
		/// </para>
		/// <para>
		/// Only context roles are considered. A principal who holds the permission globally gets nothing from this method unless they also hold the context role.
		/// </para>
		/// </remarks>
		/// <param name="datasetIds">The dataset IDs.</param>
		/// <param name="permission">The permission the current user must hold on a dataset for its grants to be returned.</param>
		/// <returns>One <see cref="DatasetContextGrants"/> per dataset that passed the filter. Empty if <paramref name="datasetIds"/> is <c>null</c> or empty, no context role grants the permission, or the user holds none on those datasets.</returns>
		Task<List<DatasetContextGrants>> RetrieveEffectiveDatasetContextGrants(IEnumerable<Guid> datasetIds, string permission);
	}
}