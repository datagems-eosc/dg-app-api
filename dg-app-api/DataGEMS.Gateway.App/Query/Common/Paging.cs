using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Query
{
	public class Paging
	{
		public int Offset { get; set; }
		public int Size { get; set; }

		public bool IsEmpty { get { return this.Offset < 0 || this.Size <= 0; } }
	}
}
