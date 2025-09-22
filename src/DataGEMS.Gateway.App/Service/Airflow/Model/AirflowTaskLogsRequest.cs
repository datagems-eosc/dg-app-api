using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	internal class AirflowTaskLogsRequest
	{  
		// Path params
		[JsonProperty("dag_id")]
		public string DagId { get; set; }

		[JsonProperty("dag_run_id")]
		public string DagRunId { get; set; }

		[JsonProperty("task_id")]
		public string TaskId { get; set; }

		[JsonProperty("try_number")]
		public int TryNumber { get; set; }

		// Query params
		[JsonProperty("full_content")]
		public bool FullContent { get; set; } = true;

		[JsonProperty("map_index")]
		public int MapIndex { get; set; } = -1;

		[JsonProperty("token")]
		public string Token { get; set; }
	}
}
