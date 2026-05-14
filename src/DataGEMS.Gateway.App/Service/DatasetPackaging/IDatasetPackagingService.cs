namespace DataGEMS.Gateway.App.Service.DatasetPackaging
{
	public interface IDatasetPackagingService
	{
		Task<HashSet<Guid>> IsInPackaging(List<Guid> datasetIds);
	}
}
