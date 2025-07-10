
namespace DataGEMS.Gateway.App.Authorization
{
	[Flags]
	public enum AuthorizationFlags
	{
		None = 1 << 0,
		Permission = 1 << 1,
		Owner = 1 << 2,
		Context = 1 << 3,
		Any = Permission | Owner | Context,
	}
}
