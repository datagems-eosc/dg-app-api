using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.Discovery.Model;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator
{
	public interface ITaskOrchestratorService
	{
		Task<IEnumerable<CrossDatasetDiscoveryResult>> CrossDatasetDiscoverySearch(Model.CrossDatasetDiscoveryRequest request);
		Task<AdHocQuery> AdHocQueryAsync(AdHocQueryEvaluate evaluate, IFieldSet fields = null);
		Task<string> AdHocQueryPreviewAsync(Guid adHocId, int lines);
		Task<List<Guid>> DatasetRecommendationAsync(Guid seedDatasetId, int n);
		Task<QueryDisambiguation> QueryDisambiguationAsync(DisambiguationInfo info);
	}

	public class DisambiguationInfo
	{
		//GOTCHA: Any changes to this model should cause the version to change
		public static string ModelVersion = "V1";
		public string Query { get; set; }
		public List<Guid> DatasetIds { get; set; }
	}
}
