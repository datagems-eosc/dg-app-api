using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common
{
	public enum DataLocationKind : short
	{
		[Description("Data is stored in a local or network file system path.")]
		File = 0,
		[Description("Data is accessible via an HTTP or HTTPS endpoint.")]
		Http = 1,
		[Description("Data is accessible via an FTP or FTPS server.")]
		Ftp = 2,
		[Description("Date is accesible via a repote location")]
		Remote = 3,
	}
}
