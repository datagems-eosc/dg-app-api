using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowTaskLog
	{
		public string Timestamp { get; set; }
		public string Event { get; set; }
	}
}
