using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.WorkflowProcess
{
	public interface IWorkflowProcessService
	{
		Task<App.Model.WorkflowProcess> ExecuteOnboardingFlow(DatasetPersist model, IFieldSet fields = null);
	}
}
