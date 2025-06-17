
namespace DataGEMS.Gateway.App.Authorization
{
	public interface IAuthorizationContentResolver
	{
		Boolean HasAuthenticated();
		String CurrentUser();

		Task<Boolean> HasPermission(params String[] permissions);
		
		ISet<String> PermissionsOfDatasetRoles(IEnumerable<String> datasetRoles);

		Task<Dictionary<Guid, HashSet<String>>> DatasetRolesForDataset(IEnumerable<Guid> datasetIds);
		Task<Dictionary<Guid, HashSet<String>>> DatasetRolesForCollection(IEnumerable<Guid> collectionIds);

		Task<List<String>> DatasetRolesOf();

		Task<List<String>> AffiliatedDatasetGroupCodes();
		Task<List<Guid>> AffiliatedDatasetIds();
	}
}
