using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common.Enum
{
	public enum DatasetState : short
	{
		[Description("Ready dataset")]
		Ready = 0,
		[Description("Loaded dataset")]
		Loaded = 1,
		[Description("Staged dataset")]
		Staged = 2,
	}
}
