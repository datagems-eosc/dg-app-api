using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum IsActive : short
	{
		[Description("Inactive entry")]
		Inactive = 0,
		[Description("Active entry")]
		Active = 1
	}
}
