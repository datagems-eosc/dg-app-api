using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Accounting
{
	public class AccountingLogConfig
	{
		public Boolean Enable { get; set; }
		public LogLevel Level { get; set; }
		public String ServiceId { get; set; }
	}
}
