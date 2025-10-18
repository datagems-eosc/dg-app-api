
namespace DataGEMS.Gateway.App.Service.AAI
{
	public class AAIConfig
	{
		public String Scope {  get; set; }
		public String BaseUrl { get; set; }
		public String HierarchyDatasetTopLevelName { get; set; }
		public String HierarchyDatasetGroupLevelName { get; set; }
		public String HierarchyDatasetDirectLevelName { get; set; }
		public List<String> SubGroupGrantNames { get; set; }
		public List<String> SubDirectGrantNames { get; set; }
		public String GroupsEndpoint { get; set; }
		public String GroupEndpoint { get; set; }
		public String GroupChildrenEndpoint { get; set; }
		public String UserGroupsEndpoint { get; set; }
		public String UserGroupsChangeEndpoint { get; set; }
	}
}
