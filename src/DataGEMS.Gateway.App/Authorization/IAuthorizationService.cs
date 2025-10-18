
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
		Task<Boolean> AuthorizeAffiliatedContext(AffiliatedContextResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeAffiliatedContextForce(AffiliatedContextResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwner(OwnedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwnerForce(OwnedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliatedContext(AffiliatedContextResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliatedContextForce(AffiliatedContextResource contextResource, params String[] permissions);
	}
}
