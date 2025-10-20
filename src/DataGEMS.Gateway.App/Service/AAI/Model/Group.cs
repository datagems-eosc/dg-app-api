using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.AAI.Model
{
	public class Group
	{
		[JsonProperty("id")]
		public String Id { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("path")]
		public string Path { get; set; }
		[JsonProperty("subGroupCount")]
		public int SubGroupCount { get; set; }
		[JsonProperty("subGroups")]
		public List<Group> SubGroups { get; set; }

		public Group LocateNameDeep(String name)
		{
			if (this.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return this;
			if (this.SubGroups != null)
			{
				foreach (Group child in this.SubGroups)
				{
					Group found = child.LocateNameDeep(name);
					if (found != null) return found;
				}
			}
			return null;
		}
	}
}
