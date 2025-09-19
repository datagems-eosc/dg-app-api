using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGEMS.Gateway.App.Common;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowTasks
	{
		public String Id { get; set; }
		public String TaskId { get; set; }
		public String DagRunId { get; set; }
		public String DagId { get; set; }
		public String MapIndex { get; set; }
		public DateTime? LogicalDate { get; set; }
		public DateTime? RunAfter { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public Decimal? Duration { get; set; }
		public String State { get; set; }
		public int? TryNumber { get; set; }
		public int? MaxTries { get; set; }
		public String TaskDisplayName { get; set; }
		public String Hostname { get; set; }
		public String Unixname { get; set; }
		public String Pool {  get; set; }
		public int PoolSlots { get; set; }
		public String? Queue { get; set; }
		public int? PriorityWeight { get; set; }
		public String? Operator { get; set; }
		public DateTime? QueuedWhen { get; set; }
		public DateTime? ScheduledWhen { get; set; }
		public int? Pid { get; set; }
		public String? Executor { get; set; }
		public String? ExecutorConfig { get; set; }
		public String? Note { get; set; }
		public String? RenderedMapIndex { get; set; }
		public Object? RenderedFields { get; set; }
		public List<Object> Trigger { get; set; }
		public List<Object> TriggererJob { get; set; }
		public List<Object> DagVersion { get; set; }
	}
}
