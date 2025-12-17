using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Service.InDataExploration;

namespace DataGEMS.Gateway.App.Service.QueryRecommender
{
	public interface IQueryRecommenderHttpService
	{
		Task<List<QueryRecommendation>> RecommendAsync(RecommenderInfo exploreInfo, IFieldSet fieldSet);
	}

	public class RecommenderInfo
	{
		//GOTCHA: Any changes to this model should cause the version to change
		public static string ModelVersion = "V1";
		public string Query { get; set; }
	}
}