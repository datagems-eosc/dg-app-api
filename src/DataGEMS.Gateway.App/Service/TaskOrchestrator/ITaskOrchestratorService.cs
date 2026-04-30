using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Discovery.Model;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator
{
	public interface ITaskOrchestratorService
	{
		Task<IEnumerable<CrossDatasetDiscoveryResult>> CrossDatasetDiscoverySearch(Model.CrossDatasetDiscoveryRequest request);
		Task<AdHocQuery> AdHocQueryAsync(AdHocQueryPersist persist, IFieldSet fields = null);
	}
}
