namespace DataGEMS.Gateway.App.Service.DatasetFileManagement.Model
{
	public class DatasetFileSet
	{
		public Guid Id { get; set; }
		public List<DatasetFile> Files { get; set; }
		public List<DatasetFileSet> DatasetFileSets { get; set; }
	}
}
