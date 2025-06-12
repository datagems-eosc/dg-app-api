using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Authorization
{
	public interface IPermissionPolicyService
	{
		ISet<String> PermissionsOf(IEnumerable<String> roles);
		ISet<String> PermissionsOfDataset(IEnumerable<String> affiliatedRoles);
		ISet<String> RolesHaving(String permission);
		ISet<String> DatasetRolesHaving(String permission);
		ISet<String> ClaimsHaving(String claim, String permission);
		ISet<String> ClientsHaving(String permission);
		Boolean AllowAnonymous(String permission);
		Boolean AllowAuthenticated(String permission);
	}
}
