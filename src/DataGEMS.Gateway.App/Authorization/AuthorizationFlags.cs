
namespace DataGEMS.Gateway.App.Authorization
{
	[Flags]
	public enum AuthorizationFlags
	{
		None = 1 << 0, // 0001
		Permission = 1 << 1, // 0010
		Owner = 1 << 2, // 0100
		Context = 1 << 3, // 1000
		Any = Permission | Owner | Context,
	}
}
