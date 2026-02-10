namespace DataGEMS.Gateway.App.Service.DataManagement.Model
{
	public class Dataset
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string License { get; set; }
		public string MimeType { get; set; }
		public long? Size { get; set; }
		public string Url { get; set; }
		public string Version { get; set; }
		public string Headline { get; set; }
		public List<string> Keywords { get; set; }
		public List<string> FieldOfScience { get; set; }
		public List<string> Language { get; set; }
		public List<string> Country { get; set; }
		public DateOnly? DatePublished { get; set; }
		public string ProfileRaw { get; set; }
		public string ArchivedAt { get; set; }
		public string ConformsTo { get; set; }
		public string CiteAs { get; set; }
		public string Status { get; set; }
	}
}
