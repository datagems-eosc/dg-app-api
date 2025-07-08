using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model
{
	public class CrossDatasetDiscovery
	{
		public String Content { get; set; }
		public String UseCase { get; set; }
		public Dataset Dataset { get; set; }
		public String SourceId { get; set; }
		public String ChunkId { get; set; }
		public String Language { get; set; }
		public double Distance { get; set; }
	}
}
