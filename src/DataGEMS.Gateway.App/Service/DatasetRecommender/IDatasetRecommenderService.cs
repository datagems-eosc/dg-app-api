namespace DataGEMS.Gateway.App.Service.DatasetRecommender
{
	public interface IDatasetRecommenderService
	{
		Task<HashSet<Guid>> IsInRecommender(List<Guid> datasetIds);
	}
}
