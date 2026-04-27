namespace DataGEMS.Gateway.App.Service.DatasetFileManagement.Model
{
	public class DatasetFileSet : DatasetObject
    {
		public List<DatasetFile> Files { get; set; }
		public List<DatasetFileSet> DatasetFileSets { get; set; }
	}
}
