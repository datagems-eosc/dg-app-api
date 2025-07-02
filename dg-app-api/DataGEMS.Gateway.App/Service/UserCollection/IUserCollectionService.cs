using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.UserCollection
{
	public interface IUserCollectionService
	{
		Task<App.Model.UserCollection> PersistAsync(App.Model.UserCollectionPersist model, IFieldSet fields = null);
		Task<App.Model.UserCollection> PersistAsync(App.Model.UserCollectionPersistDeep model, IFieldSet fields = null);
		Task<App.Model.UserCollection> PatchAsync(App.Model.UserCollectionDatasetPatch model, IFieldSet fields = null);
		Task<App.Model.UserCollection> AddAsync(Guid userCollectionId, Guid datasetId, IFieldSet fields = null);
		Task<App.Model.UserCollection> RemoveAsync(Guid userCollectionId, Guid datasetId, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
	}
}
