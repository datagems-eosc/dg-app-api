using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum UserCollectionKind : short
	{
		[Description("User entry")]
		User = 0,
		[Description("Favorites entry")]
		Favorites = 1
	}
}
