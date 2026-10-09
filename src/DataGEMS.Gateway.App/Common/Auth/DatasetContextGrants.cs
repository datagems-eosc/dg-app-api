namespace DataGEMS.Gateway.App.Common.Auth
{
	public class DatasetContextGrants
	{
		public Guid DatasetId { get; set; }

		public List<DatasetRoles> Users { get; set; }

		public List<DatasetRoles> UserGroups { get; set; }

		public class DatasetRoles
		{
			public string Id { get; set; }

			public HashSet<string> Roles { get; set; }
		}
	}
}
