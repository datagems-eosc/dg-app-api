
namespace DataGEMS.Gateway.App.Authorization
{
	public class PermissionPolicyConfig
	{
		public class PermissionClaims
		{
			public String Claim { get; set; }
			public List<String> Values { get; set; }
		}

		public class PermissionRoles
		{
			public String Permission { get; set; }
			public List<String> Roles { get; set; }
			public List<String> DatasetRoles { get; set; }
			public List<PermissionClaims> Claims { get; set; }
			public List<String> Clients { get; set; }
			public Boolean AllowAnonymous { get; set; } = false;
			public Boolean AllowAuthenticated { get; set; } = false;
		}

		public Dictionary<String, PermissionRoles> Policies { get; set; }
		public List<String> ExtendedClaims { get; set; }
	}
}
