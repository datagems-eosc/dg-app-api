using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Event
{
	public class OnEventArgs
	{
		public OnEventArgs(IEnumerable<Guid> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<Guid> Ids { get; private set; }
	}
}
