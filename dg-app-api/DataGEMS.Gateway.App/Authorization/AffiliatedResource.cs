using Cite.Tools.Common.Extensions;

namespace DataGEMS.Gateway.App.Authorization
{
	public class AffiliatedResource
	{
		public IEnumerable<String> UserIds { get; set; }
		public IEnumerable<String> AffiliatedRoles { get; set; }
		public IEnumerable<String> AffiliatedPermissions { get; set; }

		public AffiliatedResource() { }

		public AffiliatedResource(String userId) : this(userId.AsArray()) { }

		public AffiliatedResource(IEnumerable<String> userIds)
		{
			this.UserIds = userIds;
		}

		public AffiliatedResource(String userId, IEnumerable<String> affiliatedRoles) : this(userId)
		{
			this.AffiliatedRoles = affiliatedRoles;
		}

		public AffiliatedResource(String userId, IEnumerable<String> affiliatedRoles, IEnumerable<String> affiliatedPermissions) : this(userId, affiliatedRoles)
		{
			this.AffiliatedPermissions = affiliatedPermissions;
		}

		public AffiliatedResource(IEnumerable<String> userIds, IEnumerable<String> affiliatedRoles) : this(userIds)
		{
			this.AffiliatedRoles = affiliatedRoles;
		}

		public AffiliatedResource(IEnumerable<String> userIds, IEnumerable<String> affiliatedRoles, IEnumerable<String> affiliatedPermissions) : this(userIds, affiliatedRoles)
		{
			this.AffiliatedPermissions = affiliatedPermissions;
		}
	}
}
