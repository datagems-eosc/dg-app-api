
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
		DatasetRecommendationRegistering = 3,
		[Description("Dataset CDD Ingest")]
		CDD_Ingest = 4,
		[Description("Dataset Onboarding test")]
		DatasetOnboarding_test = 5,
		[Description("Dataset Profiling test")]
		DatasetProfiling_test = 6,
	}
}
