using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Authorization
{
	public interface IAuthorizationService
	{
		Task<Boolean> AuthorizeOrOwner(OwnedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwnerForce(OwnedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliated(AffiliatedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliatedForce(AffiliatedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwnerOrAffiliated(OwnedResource ownerResource, AffiliatedResource affiliatedResource, params String[] permissions);
		Task<Boolean> AuthorizeOrOwnerOrAffiliatedForce(OwnedResource ownerResource, AffiliatedResource affiliatedResource, params String[] permissions);
		Task<Boolean> Authorize(Object resource, params String[] permissions);
		Task<Boolean> Authorize(params String[] permissions);
		Task<Boolean> AuthorizeForce(Object resource, params String[] permissions);
		Task<Boolean> AuthorizeForce(params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliatedDeferred(AffiliatedDeferredResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeOrAffiliatedDeferredForce(AffiliatedDeferredResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeOwner(OwnedResource resource);
		Task<Boolean> AuthorizeOwnerForce(OwnedResource resource);
		Task<Boolean> AuthorizeAffiliated(AffiliatedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeAffiliatedForce(AffiliatedResource resource, params String[] permissions);
		Task<Boolean> AuthorizeAffiliatedDeferred(AffiliatedDeferredResource contextResource, params String[] permissions);
		Task<Boolean> AuthorizeAffiliatedDeferredForce(AffiliatedDeferredResource contextResource, params String[] permissions);
	}
}
