using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskLogList
	{
		[JsonProperty("content")]
		public List<AirflowTaskLog> Content { get; set; }
		[JsonProperty("continuation_token")]
		public String? ContinuationToken  { get; set; }
	}
}
