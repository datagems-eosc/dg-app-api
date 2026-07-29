using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.WorkflowProcess
{
	public interface IWorkflowProcessService
	{
		Task<App.Model.WorkflowProcess> ExecuteOnboardingFlow(DatasetPersist model, IFieldSet fields = null);
		Task<App.Model.WorkflowProcess> ExecuteProfilingFlow(DatasetProfiling model, IFieldSet fields = null);
		Task<App.Model.WorkflowProcess> ExecutePackagingFlow(App.Model.DatasetPackaging model, IFieldSet fields = null);
		Task<App.Model.WorkflowProcess> ExecuteRecommendationFlow(App.Model.DatasetRecommendationRegistering model, IFieldSet fields = null);
		Task<App.Model.WorkflowProcess> ExecuteCddIngestionFlow(App.Model.DatasetCddIngest model, IFieldSet fields = null);
		Task UpdateWorkflowProcessStep(WorkflowProcessStepPersist model);
		Task FinilizeOnboardingStep(WorkflowProcessStepPersist model, DatasetProfiling profiling);
		Task FinilizeProfilingStep(WorkflowProcessStepPersist model, Guid datasetId);
		Task FinilizePackagingStep(WorkflowProcessStepPersist model, Guid datasetId);
		Task FinilizeRecommendationStep(WorkflowProcessStepPersist model, Guid datasetId);
		Task FinilizeCddIngestionStep(WorkflowProcessStepPersist model, Guid datasetId);

	}
}
