using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.InDataExploration
{
	public class InDataExplorationHttpConfig
	{
		public String Scope { get; set; }
		public String BaseUrl { get; set; }
		public String GeoQueryEndpoint { get; set; }
		public String TextToSqlEndpoint { get; set; }
		public String SimpleExploreEndpoint { get; set; }
	}
}
