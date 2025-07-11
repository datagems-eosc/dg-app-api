using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class InDataTextToSqlExploration
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Sql { get; set; }
	}
}
