using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
	public interface IDatasetService
	{
		Task<Guid> FutureOnboardAsync(App.Model.DatasetPersist model, IFieldSet fields = null);
		Task<Guid> FutureProfileAsync(App.Model.DatasetProfiling model);
	}
}
