using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Common
{
	public enum Kind : short
	{
		[Description("User query")]
		UserQuery = 0,
		[Description("Response")]
		Response = 1
	}
}
