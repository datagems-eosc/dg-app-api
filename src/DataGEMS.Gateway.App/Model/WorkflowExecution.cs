using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowExecution
	{
		public String Id { get; set; }	
		public String WorkflowId { get; set;}
		public DateTime? LogicalDate { get; set; }
		public DateTime? QueuedAt { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public DateTime? DataIntervalStart { get; set; }
		public DateTime? DataIntervalEnd { get; set; }
		public DateTime? RunAfter { get; set; }
		public DateTime? LastSchedulingDecision { get; set; }
		public WorkflowRunType? RunType { get; set; }
		public String TriggeredBy { get; set; }
		public WorkflowRunState? State { get; set; }
		public String Note { get;set; }
		public String BundleVersion { get; set; }
	}
}
