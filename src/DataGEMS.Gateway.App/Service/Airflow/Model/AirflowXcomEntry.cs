using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowXcomEntry
	{
		[JsonProperty("key")]
		public String Key { get; set; }
		[JsonProperty("task_id")]
		public String TaskId { get; set; }
		[JsonProperty("run_id")]
		public String DagRunId { get; set; }
		[JsonProperty("dag_id")]
		public String DagId { get; set; }

		[JsonProperty("map_index")]
		public String MapIndex { get; set; }
		[JsonProperty("logical_date")]
		public String? LogicalDate { get; set; }
		[JsonProperty("timestamp")]
		public String? Timestamp { get; set; }

	}
}
