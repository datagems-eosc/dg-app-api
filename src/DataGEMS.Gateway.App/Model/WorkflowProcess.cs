using DataGEMS.Gateway.App.Common.Enum;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowProcess
	{
		public Guid? Id { get; set; }
		public Guid? ProcessId { get; set; }
		public User User { get; set; }
		public Dataset Dataset { get; set; }
		public List<WorkflowProcessStep> Steps { get; set; }
		public WorkflowProcessStatus? Status { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
