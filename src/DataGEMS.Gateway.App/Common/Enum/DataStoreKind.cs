using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum DataStoreKind: short
	{
		[Description("The dataset is stored in a filesystem")]
		FileSystem = 0,
		[Description("The dataset is stored in a relational database")]
		RelationalDatabase = 1,
	}
}
