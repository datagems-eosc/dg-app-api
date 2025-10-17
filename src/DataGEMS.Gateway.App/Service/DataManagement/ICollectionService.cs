using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
	public interface ICollectionService
	{
		Task<App.Model.Collection> PersistAsync(App.Model.CollectionPersist model, IFieldSet fields = null);
		Task<App.Model.Collection> PersistAsync(App.Model.CollectionPersistDeep model, IFieldSet fields = null);
		Task<App.Model.Collection> PatchAsync(App.Model.CollectionDatasetPatch model, IFieldSet fields = null);
		Task<App.Model.Collection> AddAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null);
		Task<App.Model.Collection> RemoveAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
	}
}
