using DataGEMS.Gateway.App.Service.Discovery.Model;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator
{
	public interface ITaskOrchestratorService
	{
		Task<IEnumerable<CrossDatasetDiscoveryResult>> CrossDatasetDiscoverySearch(string query);
	}
}
