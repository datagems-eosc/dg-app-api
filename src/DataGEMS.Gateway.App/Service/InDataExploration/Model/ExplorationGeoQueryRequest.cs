using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.InDataExploration.Model
{
	public class ExplorationGeoQueryRequest
	{
		[JsonProperty("question")]
		public string Question { get; set; }
	}
}
