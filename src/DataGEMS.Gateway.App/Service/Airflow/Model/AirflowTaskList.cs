using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskList
	{
		[JsonProperty("task_instances")]
		public List<AirflowTaskExecution> Items { get; set; }
		[JsonProperty("total_entries")]
		public int? TotalEntries { get; set; }
	}
}
