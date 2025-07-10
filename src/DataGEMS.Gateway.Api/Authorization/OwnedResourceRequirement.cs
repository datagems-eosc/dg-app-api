using Microsoft.AspNetCore.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	public class OwnedResourceRequirement : IAuthorizationRequirement
	{
		public OwnedResourceRequirement() { }
	}
}
