namespace DataGEMS.Gateway.App.Service.DatasetLinking
{
	public class DatasetLinkingHttpConfig
	{
		public string Scope { get; set; }
		public string BaseUrl { get; set; }
		public string StartRefineEndpoint { get; set; }
		public string JobStatusEndpoint { get; set; }
		public string JobResultEndpoint { get; set; }
	}
}
