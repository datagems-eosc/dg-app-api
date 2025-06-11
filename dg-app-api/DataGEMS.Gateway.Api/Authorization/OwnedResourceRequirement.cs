using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGEMS.Gateway.Api.Authorization
{
	public class OwnedResourceRequirement : IAuthorizationRequirement
	{
		public OwnedResourceRequirement() { }
	}
}
