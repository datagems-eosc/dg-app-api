using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.LogTracking
{
	public class LogTrackingEntryConfig
	{
		public Boolean Enabled { get; set; }
		public LogLevel Level { get; set; }
		public PrincipalEntryInfo Principal { get; set; }
		public InvokerEntryInfo Invoker { get; set; }

		public class PrincipalEntryInfo
		{
			public Boolean Subject { get; set; }
			public Boolean Username { get; set; }
			public Boolean Client { get; set; }
		}

		public class InvokerEntryInfo
		{
			public Boolean IPAddress { get; set; }
			public Boolean IPAddressFamily { get; set; }
			public Boolean RequestScheme { get; set; }
			public Boolean ClientCertificateSubjectName { get; set; }
			public Boolean ClientCertificateThumbpint { get; set; }
		}
	}
}
