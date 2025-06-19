using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model
{
	public class UserCollection
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public User User { get; set; }
		public List<UserDatasetCollection> UserDatasetCollections { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public String Hash { get; set; }
	}
}
