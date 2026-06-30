
using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum ConversationMessageKind : short
	{
		[Description("CrossDataset Query")]
		CrossDatasetQuery = 0,
		[Description("CrossDataset Response")]
		CrossDatasetResponse = 1,
		[Description("InDataExplore Query")]
		InDataExploreQuery = 2,
		[Description("InDataExplore Response")]
		InDataExploreResponse = 3,
		[Description("QueryRecommender Query")]
		QueryRecommenderQuery = 4,
		[Description("QueryRecommender Response")]
		QueryRecommenderResponse = 5,
		[Description("MathE Recommendation Query")]
		MatheRecommendationQuery = 6,
		[Description("MathE Recommendation Response")]
		MatheRecommendationResponse = 7,
		[Description("Linguistic Features Query")]
		LinguisticFeaturesQuery = 8,
		[Description("Linguistic Features Response")]
		LinguisticFeaturesResponse = 9,
	}
}
