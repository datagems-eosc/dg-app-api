using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public interface IAirflowService
	{
		Task<App.Model.WorkflowExecution> ExecuteWorkflowAsync(WorkflowExecutionArgs args, IFieldSet fields);
		Task<List<App.Model.WorkflowTaskInstance>> ExecuteTaskInstanceAsync(TaskInstanceDownstreamExecutionArgs args, IFieldSet fields);
	}
}

