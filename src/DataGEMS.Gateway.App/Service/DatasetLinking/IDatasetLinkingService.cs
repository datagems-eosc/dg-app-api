using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Service.DatasetLinking
{
	public interface IDatasetLinkingService
	{
		Task<DatasetLinkingStatus> GetJobStatusByIdAsync(Guid id);
		public Task<string> GetJobByIdAsync(Guid id);
	}
}
