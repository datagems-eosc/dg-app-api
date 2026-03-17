namespace DataGEMS.Gateway.App.Service.TaskOrchestrator.Model
{
	public class CrossDatasetDiscoveryRequest
	{
		public string Query { get; set; }
		public int ResultCount { get; set; }
		public List<Guid> DatasetIds { get; set; }
	}
}
