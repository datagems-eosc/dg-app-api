
namespace DataGEMS.Gateway.App.Service.AAI
{
	public class AAIConfig
	{
		public String Scope {  get; set; }
		public String BaseUrl { get; set; }
		public String ContextGrantGroupPrefix { get; set; }
		public String ContextGrantTypeAttributeName { get; set; }
		public String ContextGrantTypeDatasetAttributeValue { get; set; }
		public String ContextGrantTypeCollectionAttributeValue { get; set; }
		public String ContextGrantTypeUserAttributeValue { get; set; }
		public String ContextGrantTypeGroupAttributeValue { get; set; }
		public List<String> AutoAssignGrantsOnNewCollection { get; set; }
		public List<String> AutoAssignGrantsOnNewDataset { get; set; }
		public String GroupsEndpoint { get; set; }
		public String GroupRoleMappingsEndpoint { get; set; }
		public String GroupEndpoint { get; set; }
		public String GroupChildrenEndpoint { get; set; }
		public String RolesEndpoint { get; set; }
		public String UserGroupsEndpoint { get; set; }
	}
}
