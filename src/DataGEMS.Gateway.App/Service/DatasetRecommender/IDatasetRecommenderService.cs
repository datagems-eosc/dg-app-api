using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.DatasetRecommender
{
	public interface IDatasetRecommenderService
	{
		Task<HashSet<Guid>> IsInRecommender(List<Guid> datasetIds);
		Task<List<Guid>> RecommendAsync(Guid datasetId, uint? recommendationsCount);
	}
}
