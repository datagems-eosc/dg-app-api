using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model
{
	public class UserDatasetCollection
	{
		public Guid? Id { get; set; }
		public Dataset Dataset { get; set; }
		public IsActive? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public UserCollection UserCollection { get; set; }
		public String Hash { get; set; }
	}
}
