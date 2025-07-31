using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowResponse
	{
		[JsonProperty("dags")]
		public List<AirflowDagItem> Dags { get; set; }
	}

	public class AirflowDagItem
	{
		[JsonProperty("dag_id")]
		public string Dag_Id { get; set; }

		[JsonProperty("dag_name")]
		public string Dag_Name { get; set; }

		[JsonProperty("description")]
		public string Dag_Description { get; set; }
	}

}
