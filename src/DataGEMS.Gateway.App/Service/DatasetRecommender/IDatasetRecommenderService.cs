using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.DatasetRecommender
{
	public interface IDatasetRecommenderService
	{
		Task<Dictionary<Guid, bool>> ExistAsync(List<Guid> datasetIds);
	}
}
