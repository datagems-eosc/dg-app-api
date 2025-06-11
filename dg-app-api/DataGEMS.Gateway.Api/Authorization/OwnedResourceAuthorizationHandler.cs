using Microsoft.AspNetCore.Authorization;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Auth.Claims;
using DataGEMS.Gateway.App.Authorization;

namespace DataGEMS.Gateway.Api.Authorization
{
	public class OwnedResourceAuthorizationHandler : AuthorizationHandler<OwnedResourceRequirement, OwnedResource>
	{
		private readonly ILogger<OwnedResourceAuthorizationHandler> _logger;
		private readonly ClaimExtractor _extractor;

		public OwnedResourceAuthorizationHandler(
			ILogger<OwnedResourceAuthorizationHandler> logger,
			ClaimExtractor extractor)
		{
			this._logger = logger;
			this._extractor = extractor;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnedResourceRequirement requirement, OwnedResource resource)
		{
			if (context.User == null || !context.User.Claims.Any())
			{
				this._logger.Trace("current user not set");
				return Task.CompletedTask;
			}
			if (resource.UserIds == null || !resource.UserIds.Any())
			{
				this._logger.Trace("resource users not set");
				return Task.CompletedTask;
			}

			Guid? subject = this._extractor.SubjectGuid(context.User);
			if (subject.HasValue && resource.UserIds.Any(x => x == subject.Value))
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
