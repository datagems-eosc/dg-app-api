using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowXcomEntryRequest
	{
		[JsonProperty("xcpm_key")]
		public string XcomKey { get; set; }
		
		[JsonProperty("map_index")]
		public int MapIndex { get; set; }
		
		[JsonProperty("Limit")]
		public int? Limit { get; set; }

		[JsonProperty("page_offset")]
		public int? Offset { get; set; }

	}
}
