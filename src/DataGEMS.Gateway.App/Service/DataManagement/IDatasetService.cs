using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
	public interface IDatasetService
	{
		Task<App.Model.Dataset> OnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null);
		Task<App.Model.Dataset> PersistAsync(App.Model.DatasetPersist model, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
	}
}
