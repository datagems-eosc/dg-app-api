using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Discovery.Model
{
	public class CrossDatasetDiscoveryRequest
	{
		[JsonProperty("query")]
		public string Query { get; set; }
		[JsonProperty("k")]
		public int ResultCount { get; set; }
	}
}
