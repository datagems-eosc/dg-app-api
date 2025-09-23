
namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowTask
	{
		public String Id { get; set; }
		public String TaskDisplayName { get; set; }
		public String Owner { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public String TriggerRule { get; set; }
		public Boolean? DependsOnPast { get; set; }
		public Boolean? WaitForDownstream { get; set; }
		public decimal? Retries { get; set; }
		public String Queue { get; set; }
		public String Pool { get; set; }
		public decimal? PoolSlots { get; set; }
		public Object ExecutionTimeout { get; set; }
		public Object RetryDelay { get; set; }
		public Boolean? RetryExponentialBackoff { get; set; }
		public decimal? PriorityWeight { get; set; }
		public String WeightRule { get; set; }
		public String UIColor { get; set; }
		public String UIFGColor { get; set; }
		public List<String> TemplateFields { get; set; }
		public List<String> DownstreamTaskIds { get; set; }
		public String DocMd { get; set; }
		public String OperatorName { get; set; }
		public Object Params { get; set; }
		public Object ClassRef { get; set; }
		public Boolean? IsMapped { get; set; }
		public List<String> ExtraLinks { get; set; }
	}
}
