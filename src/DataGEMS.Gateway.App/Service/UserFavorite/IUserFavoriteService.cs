using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.UserFavorite
{
	public interface IUserFavoriteService
	{
		Task DeleteAsync(Guid id);
		Task<Model.UserFavorite> PersistAsync(UserFavoritePersist model, IFieldSet fields = null);
	}
}