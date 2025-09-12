using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowDagList
	{
		[JsonProperty("dags")]
		public List<AirflowDag> Items { get; set; }
		[JsonProperty("total_entries")]
		public int? TotalEntries { get; set; }
	}
}
