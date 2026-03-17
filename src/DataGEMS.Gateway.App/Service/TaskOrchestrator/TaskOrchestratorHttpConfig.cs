using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator
{
	public class TaskOrchestratorHttpConfig
	{
		public string Scope { get; set; }
		public string BaseUrl { get; set; }
		public string CrossDatasetDiscoverySearchEndpoint { get; set; }

		public String CrossDatasetDiscoveryTemplatePath { get; set; }
	}
}
