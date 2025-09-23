
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowTaskInstance
	{
		public String Id { get; set; }
		public String WorkflowId { get; set; }
		public String WorkflowTaskId { get; set; }
		public String WorkflowExecutionId { get; set; }
		public String MapIndex { get; set; }
		public DateTime? LogicalDate { get; set; }
		public DateTime? RunAfter { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public Decimal? Duration { get; set; }
		public WorkflowTaskInstanceState? State { get; set; }
		public int? TryNumber { get; set; }
		public int? MaxTries { get; set; }
		public String TaskDisplayName { get; set; }
		public String Hostname { get; set; }
		public String Unixname { get; set; }
		public String Pool {  get; set; }
		public int? PoolSlots { get; set; }
		public String Queue { get; set; }
		public DateTime? QueuedWhen { get; set; }
		public DateTime? ScheduledWhen { get; set; }
		public int? PriorityWeight { get; set; }
		public String Operator { get; set; }
		public int? Pid { get; set; }
		public String Executor { get; set; }
		public String ExecutorConfig { get; set; }
		public String Note { get; set; }
		public String RenderedMapIndex { get; set; }
		public Object RenderedFields { get; set; }
		public Object Trigger { get; set; }
		public Object TriggererJob { get; set; }
		public Object DagVersion { get; set; }
		public WorkflowDefinition Workflow { get; set; }
		public WorkflowTask Task { get; set; }
		public WorkflowExecution WorkflowExecution { get; set; }

	}
}
