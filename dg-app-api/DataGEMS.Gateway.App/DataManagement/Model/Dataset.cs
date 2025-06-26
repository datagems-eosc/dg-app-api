
namespace DataGEMS.Gateway.App.DataManagement.Model
{
	public class Dataset
	{
		public Guid Id { get; set; }
		public String Code { get; set; }
		public String Name { get; set; }
		public String Description { get; set; }
		public String License { get; set; }
		public String Url { get; set; }
		public String Version { get; set; }
		public String Headline { get; set; }
		public List<String> Keywords { get; set; }
		public List<String> FieldOfScience { get; set; }
		public List<String> Language { get; set; }
		public List<String> Country { get; set; }
		public DateOnly? DatePublished { get; set; }
		public String ProfileRaw { get; set; }
	}
}
