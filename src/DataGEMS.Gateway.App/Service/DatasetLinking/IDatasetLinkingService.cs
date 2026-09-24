using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Service.DatasetLinking
{
	public interface IDatasetLinkingService
	{
		Task<DatasetLinkingStatus> GetJobStatusByIdAsync(string id);
		public Task<string> GetJobByIdAsync(string id);
		public Task<string> RefineLinkingAsync(App.Model.DatasetLinkingRefinement model);
	}
}
