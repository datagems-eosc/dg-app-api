using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Authorization
{
	public class OwnedResource
	{
		public IEnumerable<String> UserIds { get; set; }
		public Type ResourceType { get; set; }

		public OwnedResource() { }

		public OwnedResource(String userId) : this([ userId ]) { }

		public OwnedResource(IEnumerable<String> userIds)
		{
			this.UserIds = userIds;
			this.ResourceType = null;
		}

		public OwnedResource(String userId, Type resourceType) : this([ userId ], resourceType) { }

		public OwnedResource(IEnumerable<String> userIds, Type resourceType)
		{
			this.UserIds = userIds;
			this.ResourceType = resourceType;
		}
	}
}
