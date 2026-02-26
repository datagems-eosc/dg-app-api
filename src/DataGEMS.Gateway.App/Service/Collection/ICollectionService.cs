using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.Collection
{
	public interface ICollectionService
	{
		Task<Model.Collection> PersistAsync(Model.CollectionPersist model, IFieldSet fields = null);
		Task<Model.Collection> PersistAsync(Model.CollectionPersistDeep model, IFieldSet fields = null);
		Task<Model.Collection> PatchAsync(Model.CollectionDatasetPatch model, IFieldSet fields = null);
		Task<Model.Collection> AddAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null);
		Task<Model.Collection> RemoveAsync(Guid collectionId, Guid datasetId, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
	}
}
