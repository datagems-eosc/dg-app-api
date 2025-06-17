
namespace DataGEMS.Gateway.App.Model
{
	public class Dataset
	{
		public Guid? Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }
		public List<Model.Collection> Collections { get; set; }
		public String Permissions { get; set; }
	}
}
