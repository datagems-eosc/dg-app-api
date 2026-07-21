using DataGEMS.Gateway.App.Common.Enum;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowProcessStep
	{
		public Guid? Id { get; set; }
		public WorkflowProcess Process { get; set; }
		public Guid? StepId { get; set; }
		public string WorkflowTaskInstanceDetails { get; set; }
		public WorkflowProcessStatus? Status { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
