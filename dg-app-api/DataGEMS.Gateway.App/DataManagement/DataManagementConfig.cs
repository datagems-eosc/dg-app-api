using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.DataManagement
{
	public class DataManagementConfig
	{
		public String Scope { get; set; }
		public String BaseUrl { get; set; }
		public String DatasetQueryEndpoint { get; set; }
		public String DatasetCountEndpoint { get; set; }
	}
}
