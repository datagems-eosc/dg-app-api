
namespace DataGEMS.Gateway.App.Authorization
{
	public interface IAuthorizationContentResolver
	{
		Boolean HasAuthenticated();
		String CurrentUser();

		Task<Guid?> CurrentUserId();
		Task<String> SubjectIdOfCurrentUser();
		Task<String> SubjectIdOfUserId(Guid? userId);
		Task<String> SubjectIdOfUserIdentifier(String userIdentifier);

		Task<Boolean> HasPermission(params String[] permissions);

		ISet<String> PermissionsOfContextRoles(IEnumerable<String> roles);

		Task<HashSet<String>> ContextRolesForCollectionOfUser(Guid collectionId);
		Task<HashSet<String>> ContextRolesForCollectionOfUser(String subjectId, Guid collectionId);
		Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollectionOfUser(IEnumerable<Guid> collectionIds);
		Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollectionOfUser(String subjectId, IEnumerable<Guid> collectionIds);
		Task<HashSet<String>> ContextRolesForCollectionOfUserGroup(String groupId, Guid collectionId);
		Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollectionOfUserGroup(String groupId, IEnumerable<Guid> collectionIds);

		Task<HashSet<String>> EffectiveContextRolesForDatasetOfUser(Guid datasetId);
		Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDatasetOfUser(IEnumerable<Guid> datasetIds);
		Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDatasetOfUser(String subjectId, IEnumerable<Guid> datasetIds);
		Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDatasetOfUserGroup(String groupId, IEnumerable<Guid> datasetIds);

		Task<List<String>> ContextRolesOf();

		Task<List<Guid>> ContextAffiliatedCollections(String permissions);
		Task<List<Guid>> EffectiveContextAffiliatedDatasets(String permissions);
	}
}
