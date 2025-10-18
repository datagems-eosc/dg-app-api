
namespace DataGEMS.Gateway.App.Authorization
{
	public class AffiliatedContextResource
	{
		public IEnumerable<String> AffiliatedRoles { get; set; }
		public IEnumerable<String> AffiliatedPermissions { get; set; }

		public AffiliatedContextResource() { }

		public AffiliatedContextResource(IEnumerable<String> affiliatedRoles) : this()
		{
			this.AffiliatedRoles = affiliatedRoles;
		}

		public AffiliatedContextResource(IEnumerable<String> affiliatedRoles, IEnumerable<String> affiliatedPermissions) : this(affiliatedRoles)
		{
			this.AffiliatedPermissions = affiliatedPermissions;
		}
	}
}
