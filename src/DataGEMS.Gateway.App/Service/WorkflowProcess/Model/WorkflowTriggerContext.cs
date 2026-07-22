namespace DataGEMS.Gateway.App.Service.WorkflowProcess
{
	public class WorkflowTriggerContext
	{
		public App.Model.Dataset Dataset { get; set; }
		public Airflow.Model.AirflowDag Definition { get; set; }
		public Guid WorkflowProcessId { get; set; }
		public IReadOnlyList<Data.WorkflowProcessStep> Steps { get; set; }
	}
}
