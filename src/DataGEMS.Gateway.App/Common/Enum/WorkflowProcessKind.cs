using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum WorkflowProcessKind : short
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
		CDD_Ingest = 5
	}
}
