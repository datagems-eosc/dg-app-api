using DataGEMS.Gateway.App.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Censor
{
	public class CensorContext
	{
		public CensorBehavior Behavior { get; set; }

		public static CensorContext Build(CensorBehavior behavior)
		{
			return new CensorContext { Behavior = behavior };
		}

		public static CensorContext AsCensor()
		{
			return new CensorContext { Behavior = CensorBehavior.Censor };
		}

		public static CensorContext AsThrow()
		{
			return new CensorContext { Behavior = CensorBehavior.Throw };
		}
	}
}
