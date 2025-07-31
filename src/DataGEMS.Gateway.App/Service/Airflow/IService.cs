using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public interface IAirflowService
	{
		Task<List<DataGEMS.Gateway.App.Model.Airflow>> GetDagRunsAsync(AirflowInfo request, FieldSet fieldset);
	}

	public class AirflowInfo
	{
		public String Dag_Id {  get; set; }

		public String Dag_name { get; set; }
	}
}
