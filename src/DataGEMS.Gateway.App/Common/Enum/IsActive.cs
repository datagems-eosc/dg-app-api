using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Common
{
	public enum IsActive : short
	{
		[Description("Inactive entry")]
		Inactive = 0,
		[Description("Active entry")]
		Active = 1
	}
}
