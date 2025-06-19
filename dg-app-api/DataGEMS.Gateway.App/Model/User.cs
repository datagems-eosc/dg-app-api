
namespace DataGEMS.Gateway.App.Model
{
	public class User
	{
		public Guid? Id { get; set; }
		public String Name { get; set; }
		public String Email { get; set; }
		public String IdpSubjectId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public List<UserCollection> UserCollections { get; set; }
		public String Hash { get; set; }
	}
}
