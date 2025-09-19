using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowTaskLogs
	{
		public string Timestamp { get; set; }
		public string Event { get; set; }
		public String Id { get; set; }
		public String TaskId { get; set; }
		public String DagRunId { get; set; }
		public String DagId { get; set; }
		public String MapIndex { get; set; }
		public int TryNumber { get; set; }
		public bool FullContent { get; set; } 
		public string? Token { get; set; }
	}
}
