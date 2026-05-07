using DataGEMS.Gateway.App.Service.DatasetFileManagement.Model;

namespace DataGEMS.Gateway.App.Service.DatasetFileManagement
{
	public interface IDatasetFileManagementService
	{
		Task<DatasetObject> BrowseDatasetFilesAsync(Guid datasetId, Guid? nodeId);
		Task<FileDetails> DownloadDatasetFileAsync(Guid datasetId, Guid fileObjectNodeId);
	}
}