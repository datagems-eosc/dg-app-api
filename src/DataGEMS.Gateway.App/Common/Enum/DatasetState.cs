using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common.Enum
{
	public enum DatasetState : short
	{
		[Description("Ready dataset")]
		ready = 0,
		[Description("Loaded dataset")]
		loaded = 1,
		[Description("Staged dataset")]
		staged = 2,
	}
}
