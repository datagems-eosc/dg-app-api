using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum DataStoreKind: short
	{
		[Description("The profiler will handle a filesystem")]
		RawDataPath = 0,
		[Description("The profiler will handle a relational database")]
		DatabaseConnection = 1,
	}
}
