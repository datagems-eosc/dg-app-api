
namespace DataGEMS.Gateway.App.Model
{
	public class Collection
	{
		public Guid? Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }
		public List<Model.Dataset> Datasets { get; set; }
		public int? DatasetCount { get; set; }
	}
}
