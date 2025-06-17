
namespace DataGEMS.Gateway.App.Authorization
{
	public interface IAuthorizationService
	{
		Task<Boolean> Authorize(params String[] permissions);
		Task<Boolean> Authorize(Object resource, params String[] permissions);
		Task<Boolean> AuthorizeForce(params String[] permissions);
		Task<Boolean> AuthorizeForce(Object resource, params String[] permissions);
		Task<Boolean> AuthorizeOwner(OwnedResource resource);
		Task<Boolean> AuthorizeOwnerForce(OwnedResource resource);
		Task<Boolean> AuthorizeAffiliatedDataset(AffiliatedDatasetResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeAffiliatedDatasetForce(AffiliatedDatasetResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwner(OwnedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwnerForce(OwnedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliatedDataset(AffiliatedDatasetResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliatedDatasetForce(AffiliatedDatasetResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwnerOrAffiliatedDataset(OwnedResource ownerResource, AffiliatedDatasetResource affiliatedResource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwnerOrAffiliatedDatasetForce(OwnedResource ownerResource, AffiliatedDatasetResource affiliatedResource, params String[] permissions);
	}
}
