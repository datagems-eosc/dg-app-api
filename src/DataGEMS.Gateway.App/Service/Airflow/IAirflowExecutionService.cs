using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public interface IAirflowExecutionService
	{
		Task<App.Model.WorkflowExecution> ExecutionDagAsync(string dagId, IFieldSet fields);

		Task<App.Model.WorkflowListExecution> ListofExecutionAsync(string dagId, IFieldSet fieldSet);
	}
}

