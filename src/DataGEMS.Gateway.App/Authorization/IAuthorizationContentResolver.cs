
namespace DataGEMS.Gateway.App.Authorization
{
	public interface IAuthorizationContentResolver
	{
		Boolean HasAuthenticated();
		String CurrentUser();

		Task<Guid?> CurrentUserId();
		Task<String> SubjectIdOfCurrentUser();
		Task<String> SubjectIdOfUserId(Guid? userId);

		Task<Boolean> HasPermission(params String[] permissions);

		ISet<String> PermissionsOfContextRoles(IEnumerable<String> roles);

		Task<HashSet<String>> ContextRolesForCollection(Guid collectionId);
		Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollection(IEnumerable<Guid> collectionIds);
		Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDataset(IEnumerable<Guid> datasetIds);

		Task<List<String>> ContextRolesOf();

		Task<List<Guid>> ContextAffiliatedCollections(String permissions);
		Task<List<Guid>> EffectiveContextAffiliatedDatasets(String permissions);
	}
}
