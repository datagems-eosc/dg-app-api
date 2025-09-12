using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public interface IAirflowService
	{
		Task<App.Model.WorkflowExecution> ExecuteWorkflowAsync(WorkflowExecutionArgs args, IFieldSet fields);
	}
}

