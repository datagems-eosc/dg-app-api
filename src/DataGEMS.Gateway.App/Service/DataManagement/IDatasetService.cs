using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
	public interface IDatasetService
	{
		Task<Guid> OnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null);
		Task<Guid> ProfileAsync(App.Model.DatasetProfiling model);
	}
}
