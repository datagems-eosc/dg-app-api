namespace DataGEMS.Gateway.App.Common
{
	public class DataLocation
	{
		public DataLocationKind Kind { get; set; }
		public string KindName => Kind.ToString(); //No builder maps Kind
		public string Location { get; set; }
	}
}
