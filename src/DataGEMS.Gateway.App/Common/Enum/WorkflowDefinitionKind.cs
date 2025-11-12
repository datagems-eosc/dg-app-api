
using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum WorkflowDefinitionKind : short
	{
		[Description("Dataset Onboarding")]
		DatasetOnboarding = 0,
		[Description("Dataset Profiling")]
		DatasetProfiling = 1,
		[Description("Future Dataset Onboarding")]
		DatasetOnboardingFuture = 2,
	}
}
