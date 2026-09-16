
using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum WorkflowDefinitionKind : short
	{
		[Description("Dataset Onboarding")]
		DatasetOnboarding = 0,
		[Description("Dataset Profiling")]
		DatasetProfiling = 1,
		[Description("Dataset Linking Report")]
		DatasetLinkingReport = 2,
		[Description("Dataset Packaging")]
		DatasetPackaging = 3,
		[Description("Dataset Recommendation Registering")]
		DatasetRecommendationRegistering = 4,
		[Description("Dataset CDD Ingest")]
		CDD_Ingest = 5,
		[Description("Dataset Onboarding test")]
		DatasetOnboarding_test = 6,
		[Description("Dataset Profiling test")]
		DatasetProfiling_test = 7,
		[Description("Dataset Packaging test")]
		DatasetPackaging_test = 8,
		[Description("Dataset Recommendation Registering test")]
		DatasetRecommendationRegistering_test = 9,
		[Description("Dataset CDD Ingest test")]
		CDD_Ingest_test = 10,
	}
}
