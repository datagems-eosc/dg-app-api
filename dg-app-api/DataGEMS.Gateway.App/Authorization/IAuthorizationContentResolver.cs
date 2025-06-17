
namespace DataGEMS.Gateway.App.Authorization
{
	public interface IAuthorizationContentResolver
	{
		Boolean HasAuthenticated();
		String CurrentUser();

		Task<Boolean> HasPermission(params String[] permissions);

		Task<List<String>> DatasetRolesOf();
		Task<List<String>> AffiliatedDatasetGroupCodes();
		Task<List<Guid>> AffiliatedDatasetIds();
	}
}
