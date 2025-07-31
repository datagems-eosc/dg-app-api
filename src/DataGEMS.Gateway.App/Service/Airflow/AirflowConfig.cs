using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public class AirflowConfig
	{
		public String DagListEndpoint {  get; set; }
		public String BaseUrl { get; set; }

		public String username { get; set; }
		public String password { get; set; }

	}
}
