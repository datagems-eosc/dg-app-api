using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.Airflow.Model
{
	public class AirflowTaskListRequest
	{
		[JsonProperty("dag_ids")]
		public List<string> DagIds { get; set; }
		[JsonProperty("dag_run_ids")]
		public List<string> DagRunIds { get; set; }
		[JsonProperty("task_ids")]
		public List<string> TaskIds { get; set; }

		[JsonProperty("state")]
		public List<string> State { get; set; }
		[JsonProperty("run_after_gte")]
		public DateTime? RunAfterGte { get; set; }

		[JsonProperty("run_after_lte")]
		public DateTime? RunAfterLte { get; set; }

		[JsonProperty("logical_date_gte")]
		public DateTime? LogicalDateGte { get; set; }

		[JsonProperty("logical_date_lte")]
		public DateTime? LogicalDateLte { get; set; }

		[JsonProperty("start_date_gte")]
		public DateTime? StartDateGte { get; set; }

		[JsonProperty("start_date_lte")]
		public DateTime? StartDateLte { get; set; }

		[JsonProperty("end_date_gte")]
		public DateTime? EndDateGte { get; set; }

		[JsonProperty("end_date_lte")]
		public DateTime? EndDateLte { get; set; }
		
		[JsonProperty("duration_gte")]
		public Decimal? DurationGte { get; set; }
		
		[JsonProperty("duration_lte")]
		public Decimal? DurationLte { get; set; }
		
		[JsonProperty("pool")]
		public List<String?> Pool { get; set; }
		
		[JsonProperty("queue")]
		public List<String?> Queue { get; set; }
		
		[JsonProperty("executor")]
		public List<String?> Executor { get; set; }
		
		[JsonProperty("page_offset")]
		public int? Offset { get; set; }
		
		[JsonProperty("page_limit")]
		public int? Limit { get; set; }
		
		[JsonProperty("order_by")]
		public string OrderBy { get; set; }
	}
}
