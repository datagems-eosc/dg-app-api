using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowXcomEntry
	{	
		public String Key { get; set; }
		public String Timestamp { get; set; }
		public String LogicalDate { get; set; }
		public String MapIndex { get; set; }
		public String WorkflowId { get; set; } //dag id
		public String WorkflowTaskId { get; set; } //task id
		public String WorkflowExecutionId { get; set; } //dag run id
	}
}
