using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class CrossDatasetDiscovery
	{
		public string Content { get; set; }
		public string UseCase { get; set; }
		public Dataset Dataset { get; set; }
		public string ChunkId { get; set; }
		public string Language { get; set; }
		public double Distance { get; set; }
	}
}
