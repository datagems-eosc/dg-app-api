using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Authorization
{
	public class AffiliatedDeferredResource
	{
		public IEnumerable<Guid> UserIds { get; set; }
		public IEnumerable<String> AffiliatedRoles { get; set; }
		public IEnumerable<String> AffiliatedPermissions { get; set; }

		public AffiliatedDeferredResource() { }

		public AffiliatedDeferredResource(Guid userId) : this([ userId ]) { }

		public AffiliatedDeferredResource(IEnumerable<Guid> userIds)
		{
			this.UserIds = userIds;
		}

		public AffiliatedDeferredResource(Guid userId, IEnumerable<String> affiliatedRoles) : this(userId)
		{
			this.AffiliatedRoles = affiliatedRoles;
		}

		public AffiliatedDeferredResource(Guid userId, IEnumerable<String> affiliatedRoles, IEnumerable<String> affiliatedPermissions) : this(userId, affiliatedRoles)
		{
			this.AffiliatedPermissions = affiliatedPermissions;
		}

		public AffiliatedDeferredResource(IEnumerable<Guid> userIds, IEnumerable<String> affiliatedRoles) : this(userIds)
		{
			this.AffiliatedRoles = affiliatedRoles;
		}

		public AffiliatedDeferredResource(IEnumerable<Guid> userIds, IEnumerable<String> affiliatedRoles, IEnumerable<String> affiliatedPermissions) : this(userIds, affiliatedRoles)
		{
			this.AffiliatedPermissions = affiliatedPermissions;
		}
	}
}
