using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
	public interface IDatasetService
	{
		Task<Guid> OnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null);
		Task<Guid> FutureOnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null);
		Task<Guid> ProfileAsync(Guid id);
		Task<Guid> FutureProfileAsync(Guid id);
		Task<App.Model.Dataset> PersistAsync(App.Model.DatasetPersist model, IFieldSet fields = null);
		Task DeleteAsync(Guid id);

		Task<Guid> OnboardAsDataManagementAsync(App.Model.DatasetPersist model);
		Task<Guid> UpdateProfileAsDataManagementAsync(Guid id, String profile);
	}
}
