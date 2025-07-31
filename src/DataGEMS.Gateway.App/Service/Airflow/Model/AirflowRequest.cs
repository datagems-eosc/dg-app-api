using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowRequest
	{
		[JsonProperty("Dag_Id")]
		public string dag_id { get; set; }
	}
}
