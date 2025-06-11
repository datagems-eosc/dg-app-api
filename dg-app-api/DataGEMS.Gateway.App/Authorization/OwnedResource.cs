using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Authorization
{
	public class OwnedResource
	{
		public IEnumerable<Guid> UserIds { get; set; }
		public Type ResourceType { get; set; }

		public OwnedResource() { }

		public OwnedResource(Guid userId) : this([ userId ]) { }

		public OwnedResource(IEnumerable<Guid> userIds)
		{
			this.UserIds = userIds;
			this.ResourceType = null;
		}

		public OwnedResource(Guid userId, Type resourceType) : this([ userId ], resourceType) { }

		public OwnedResource(IEnumerable<Guid> userIds, Type resourceType)
		{
			this.UserIds = userIds;
			this.ResourceType = resourceType;
		}
	}
}
