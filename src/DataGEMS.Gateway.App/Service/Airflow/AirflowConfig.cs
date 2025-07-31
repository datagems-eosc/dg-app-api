using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public class AirflowConfig
	{
		public String BaseUrl { get; set; }
		public String TokenEndpoint { get; set; }
		public String DagListEndpoint { get; set; }
		public String DagByIdEndpoint { get; set; }
		public String Username { get; set; }
		public String Password { get; set; }

	}
}
