using DataGEMS.Gateway.App.Service.DatasetFileManagement.Model;

namespace DataGEMS.Gateway.App.Service.DatasetFileManagement
{
	public interface IDatasetFileManagementService
	{
		Task<DatasetFileSet> BrowseDatasetFilesAsync(Guid datasetId, Guid? nodeId);
		Task<byte[]> DownloadDatasetFileAsync(Guid datasetId, Guid fileObjectNodeId);
	}
}