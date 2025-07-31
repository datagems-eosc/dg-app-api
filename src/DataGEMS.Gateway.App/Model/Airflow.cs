using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model
{
	public class Airflow
	{
		public String Dag_Id { get; set; }

		public String Dag_Name { get; set; }

		public String Dag_Description { get; set; }

	}
}
