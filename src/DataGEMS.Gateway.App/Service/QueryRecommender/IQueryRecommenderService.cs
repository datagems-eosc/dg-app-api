using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.QueryRecommender
{
	public interface IQueryRecommenderService
	{
		Task<List<QueryRecommendation>> RecommendAsync(RecommenderInfo recommendInfo, IFieldSet fieldSet);
	}

	public class RecommenderInfo
	{
		//GOTCHA: Any changes to this model should cause the version to change
		public static string ModelVersion = "V1";
		public string Query { get; set; }
	}
}