using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum WorkflowTaskInstanceState : short
	{
		[Description("Removed")]
		Removed = 0,
		[Description("Scheduled")]
		Scheduled = 1,
		[Description("Queued")]
		Queued = 2,
		[Description("Running")]
		Running = 3,
		[Description("Success")]
		Success = 4,
		[Description("Restarting")]
		Restarting = 5,
		[Description("Failed")]
		Failed = 6,
		[Description("Up_for_retry")]
		Up_for_retry = 7,
		[Description("Up_for_reschedule")]
		Up_for_reschedule = 8,
		[Description("Upstream_failed")]
		Upstream_failed = 9,
		[Description("Skipped")]
		Skipped = 10,
		[Description("Deferred")]
		Deferred = 11
	}
}
