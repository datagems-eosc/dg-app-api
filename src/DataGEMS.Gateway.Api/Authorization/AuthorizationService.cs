using Cite.WebTools.CurrentPrincipal;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Exception;

namespace DataGEMS.Gateway.Api.Authorization
{
	//GOTCHA: THe "match all" is set to false. This is alligned with the UI presentation logic to show / hide sections. If you change it, make sure to properly propagate the logic
	public class AuthorizationService : DataGEMS.Gateway.App.Authorization.IAuthorizationService
	{
		private readonly Microsoft.AspNetCore.Authorization.IAuthorizationService _authorizationService;
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly ILogger<AuthorizationService> _logger;
		private readonly ErrorThesaurus _errors;

		public AuthorizationService(
			ILogger<AuthorizationService> logger,
			Microsoft.AspNetCore.Authorization.IAuthorizationService authorizationService,
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			ErrorThesaurus errors)
		{
			this._logger = logger;
			this._authorizationService = authorizationService;
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._errors = errors;
		}

		public async Task<Boolean> Authorize(params String[] permissions)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new PermissionAuthorizationRequirement(permissions.ToList(), matchAll: false)).Build();
			return await this.Authorize(force: false, resource: null, policy: policy);
		}

		public async Task<Boolean> Authorize(Object resource, params String[] permissions)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new PermissionAuthorizationRequirement(permissions.ToList(), matchAll: false)).Build();
			return await this.Authorize(force: false, resource: resource, policy: policy);
		}

		public async Task<Boolean> AuthorizeForce(params String[] permissions)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new PermissionAuthorizationRequirement(permissions.ToList(), matchAll: false)).Build();
			return await this.Authorize(force: true, resource: null, policy: policy);
		}

		public async Task<Boolean> AuthorizeForce(Object resource, params String[] permissions)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new PermissionAuthorizationRequirement(permissions.ToList(), matchAll: false)).Build();
			return await this.Authorize(force: true, resource: resource, policy: policy);
		}

		public async Task<Boolean> AuthorizeOwner(OwnedResource resource)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new OwnedResourceRequirement()).Build();
			return await this.Authorize(force: false, resource: resource, policy: policy);
		}

		public async Task<Boolean> AuthorizeOwnerForce(OwnedResource resource)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new OwnedResourceRequirement()).Build();
			return await this.Authorize(force: true, resource: resource, policy: policy);
		}

		public async Task<Boolean> AuthorizeAffiliatedContext(AffiliatedContextResource resource, params String[] permissions)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new AffiliatedContextAuthorizationRequirement(requiredPermissions: permissions.ToList(), matchAll: false)).Build();
			return await this.Authorize(force: false, resource: resource, policy: policy);
		}

		public async Task<Boolean> AuthorizeAffiliatedContextForce(AffiliatedContextResource resource, params String[] permissions)
		{
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder().AddRequirements(new AffiliatedContextAuthorizationRequirement(requiredPermissions: permissions.ToList(), matchAll: false)).Build();
			return await this.Authorize(force: true, resource: resource, policy: policy);
		}

		public async Task<Boolean> AuthorizeOrOwner(OwnedResource resource, params String[] permissions)
		{
			Boolean isAuthorized = await this.Authorize(permissions);
			if (!isAuthorized && resource != null) isAuthorized = await this.AuthorizeOwner(resource);
			return isAuthorized;
		}

		public async Task<Boolean> AuthorizeOrOwnerForce(OwnedResource resource, params String[] permissions)
		{
			if (resource == null) return await this.AuthorizeForce(permissions);
			Boolean isAuthorized = await this.Authorize(permissions);
			if (isAuthorized) return true;
			return await this.AuthorizeOwnerForce(resource);
		}

		public async Task<Boolean> AuthorizeOrAffiliatedContext(AffiliatedContextResource contextResource, params String[] permissions)
		{
			Boolean isAuthorized = await this.Authorize(permissions);
			if (!isAuthorized && contextResource != null) isAuthorized = await this.AuthorizeAffiliatedContext(contextResource, permissions);
			return isAuthorized;
		}

		public async Task<Boolean> AuthorizeOrAffiliatedContextForce(AffiliatedContextResource contextResource, params String[] permissions)
		{
			if (contextResource == null) return await this.AuthorizeForce(permissions);
			Boolean isAuthorized = await this.Authorize(permissions);
			if (isAuthorized) return true;
			return await this.AuthorizeAffiliatedContextForce(contextResource, permissions);
		}

		private async Task<Boolean> Authorize(Boolean force, Object resource, AuthorizationPolicy policy)
		{
			ClaimsPrincipal currentPrincipal = this._currentPrincipalResolverService.CurrentPrincipal();

			AuthorizationResult result = null;
			if (resource == null) result = await this._authorizationService.AuthorizeAsync(currentPrincipal, policy);
			else result = await this._authorizationService.AuthorizeAsync(currentPrincipal, resource, policy);

			LogLevel level = result.Succeeded ? LogLevel.Trace : (force ? LogLevel.Warning : LogLevel.Trace);
			this._logger.LogSafe(level, new MapLogEntry("checking current principal").And("policy", policy).And("resource", resource).And("success", result.Succeeded).And("force", force));

			if (!result.Succeeded && force) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);
			return result.Succeeded;
		}
	}
}
