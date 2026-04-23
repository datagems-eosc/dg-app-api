namespace DataGEMS.Gateway.App.Service.DatasetFileManagement
{
	public interface IDatasetFileManagementService
	{
		Task<byte[]> DownloadDatasetFileAsync(Guid datasetId, Guid fileObjectNodeId);
	}
}