using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public interface IAirflowAccessTokenService
	{
		Task<string> GetAirflowAccessTokenAsync();
	}
}
