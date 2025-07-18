
namespace DataGEMS.Gateway.App.Service.UserCollection
{
	public class UserCollectionConfig
	{
		public FavoritesInfo Favorites {  get; set; }

		public class FavoritesInfo
		{
			public Boolean BootstrapFavorites { get; set; }
			public String CollectionName { get; set; }
		}
	}
}
