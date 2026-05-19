using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.DatasetPackaging
{
	public interface IDatasetPackagingService
	{
		Task<HashSet<Guid>> IsInPackaging(List<Guid> datasetIds);
		Task<PackageRecommendation> RecommendAsync(PackageRecommendationRequest request, IFieldSet censoredFields);


	}
}
