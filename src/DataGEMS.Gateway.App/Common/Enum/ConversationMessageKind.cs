
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
	}
}
