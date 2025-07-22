
using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum ConversationMessageKind : short
	{
		[Description("CrossDataset Query")]
		CrossDatasetQuery = 0,
		[Description("CrossDataset Response")]
		CrossDatasetResponse = 1,
		[Description("InDataGeo Query")]
		InDataGeoQuery = 2,
		[Description("InDataGeo Response")]
		InDataGeoResponse = 3,
		[Description("InDataTextToSql Query")]
		InDataTextToSqlQuery = 4,
		[Description("InDataTextToSql Response")]
		InDataTextToSqlResponse = 5,
		[Description("InDataSimpleExplore Query")]
		InDataSimpleExploreQuery = 6,
		[Description("InDataSimpleExplore Response")]
		InDataSimpleExploreResponse = 7,
	}
}
