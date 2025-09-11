using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowExecution
	{
		public String Id { get; set; }	
		public String RunId { get; set;}
		public DateTime? QueuedAt { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public DateTime? IntervalStart { get; set; }
		public DateTime? IntervalEnd { get; set; }
		public DateTime? LogicalDate { get; set; }
		public DateTime? RunAfter { get; set; }
		public DateTime? LastSchedulingDecision { get; set; }
		public String RunType { get; set; }
		public String TriggeredBy { get; set; }
		public String State { get; set; }
		public Object Conf { get; set; }
		public List<Object> DagVersions { get; set; }
		public String Note { get;set; }
		public String BundleVersion { get; set; }
	}
}
