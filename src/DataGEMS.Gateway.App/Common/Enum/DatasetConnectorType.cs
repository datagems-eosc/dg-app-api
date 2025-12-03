using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum DatasetConnectorType: short
	{
		[Description("The files are in raw form")]
		RawDataPath = 0,
		[Description("The files contain database records")]
		DatabaseConnection = 1,
	}
}
