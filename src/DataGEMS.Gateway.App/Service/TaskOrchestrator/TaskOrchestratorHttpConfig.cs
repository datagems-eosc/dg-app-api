namespace DataGEMS.Gateway.App.Service.TaskOrchestrator
{
	public class TaskOrchestratorHttpConfig
	{
		public string Scope { get; set; }
		public string BaseUrl { get; set; }
		public string CrossDatasetDiscoverySearchEndpoint { get; set; }
		public string AdHocQueryEndpoint { get; set; }

		public String CrossDatasetDiscoveryTemplatePath { get; set; }
	}
}
