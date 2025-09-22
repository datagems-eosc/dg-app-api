using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowXcomEntryList
	{
		[JsonProperty("xcom_entries")]
		public List<AirflowXcomEntry> Items { get; set; }

		[JsonProperty("total_entries")]
		public int TotalEntries { get; set; }
		
	}
}
