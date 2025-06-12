

namespace DataGEMS.Gateway.App.Authorization
{
	public class AffiliatedDatasetResource
	{
		public IEnumerable<String> AffiliatedRoles { get; set; }
		public IEnumerable<String> AffiliatedPermissions { get; set; }

		public AffiliatedDatasetResource() { }

		public AffiliatedDatasetResource(IEnumerable<String> affiliatedRoles) : this()
		{
			this.AffiliatedRoles = affiliatedRoles;
		}

		public AffiliatedDatasetResource(IEnumerable<String> affiliatedRoles, IEnumerable<String> affiliatedPermissions) : this(affiliatedRoles)
		{
			this.AffiliatedPermissions = affiliatedPermissions;
		}
	}
}
