using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskLogs
	{
		[JsonProperty("timestamp")]
		public String Timestamp { get; set; }
		[JsonProperty("event")]
		public String Event { get; set; }

	}
}
