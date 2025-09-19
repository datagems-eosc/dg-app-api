using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum WorkflowRunType : short
	{
		[Description("Backfill")]
		Backfill = 0,
		[Description("Scheduled")]
		Scheduled = 1,
		[Description("Manual")]
		Manual = 2,
		[Description("Asset_triggered")]
		Asset_triggered = 3,
	}
}
