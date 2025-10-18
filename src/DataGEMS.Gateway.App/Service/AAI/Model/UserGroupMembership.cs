using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.AAI.Model
{
	public class UserGroupMembership
	{
		[JsonProperty("id")]
		public String Id { get; set; }
		[JsonProperty("name")]
		public String Name { get; set; }
		[JsonProperty("path")]
		public String Path { get; set; }
		[JsonProperty("parentId")]
		public String ParentId { get; set; }
		[JsonProperty("subGroups")]
		public List<UserGroupMembership> SubGroups { get; set; }
	}
}
