namespace DataGEMS.Gateway.App.Service.DatasetFileManagement.Model
{
	public class DatasetFile : DatasetObject
	{
		public string Name { get; set; }
		public string Sha256 { get; set; }
		public string Size { get; set; }
		public string MimeType { get; set; }
		public string Description { get; set; }
	}
}
