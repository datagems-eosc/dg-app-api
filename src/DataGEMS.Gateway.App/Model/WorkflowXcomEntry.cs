namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowXcomEntry
	{	
		public String Key { get; set; }
		public DateTime? Timestamp { get; set; }
		public DateTime? LogicalDate { get; set; }
		public int? MapIndex { get; set; }
		public String WorkflowId { get; set; }
		public String WorkflowTaskId { get; set; }
		public String WorkflowExecutionId { get; set; }
		public String Value { get; set; }
	}
}
