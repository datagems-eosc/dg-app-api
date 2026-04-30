
using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum WorkflowDefinitionKind : short
	{
		[Description("Dataset Onboarding")]
		DatasetOnboarding = 0,
		[Description("Dataset Profiling")]
		DatasetProfiling = 1,
		[Description("Dataset Packaging")]
		DatasetPackaging = 2,
		[Description("Dataset Recommendation Registering")]
		DatasetRecommendationRegistering = 3
	}
}
