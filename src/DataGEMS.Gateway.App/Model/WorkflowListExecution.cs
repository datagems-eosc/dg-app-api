using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGEMS.Gateway.App.Service.Airflow.Model;

namespace DataGEMS.Gateway.App.Model
{
	public class WorkflowListExecution
	{
		public List<AirflowDagExecution> DagRuns { get; set; }
		public int TotalEntries { get; set; }
	}
}
