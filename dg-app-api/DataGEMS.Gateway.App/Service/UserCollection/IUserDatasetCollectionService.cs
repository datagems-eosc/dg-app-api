using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.UserCollection
{
	public interface IUserDatasetCollectionService
	{
		Task<List<Guid>> ApplyEditAccess(List<Guid> ids);
		Task<List<Guid>> ApplyDeleteAccess(List<Guid> ids);
		Task<Model.UserDatasetCollection> PersistAsync(Model.UserDatasetCollectionPersist model, IFieldSet fields = null);
		Task<List<Model.UserDatasetCollection>> PersistAsync(IEnumerable<Model.UserDatasetCollectionPersist> models, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
		Task DeleteAsync(IEnumerable<Guid> ids);
	}
}
