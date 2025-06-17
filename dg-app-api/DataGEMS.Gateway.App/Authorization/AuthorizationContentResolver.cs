using Cite.Tools.Auth.Claims;
using Cite.WebTools.CurrentPrincipal;

namespace DataGEMS.Gateway.App.Authorization
{
	public class AuthorizationContentResolver : IAuthorizationContentResolver
	{
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly IAuthorizationService _authorizationService;
		private readonly ClaimExtractor _extractor;
		private readonly IPermissionPolicyService _permissionPolicyService;

		public AuthorizationContentResolver(
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			IAuthorizationService authorizationService,
			IPermissionPolicyService permissionPolicyService,
			ClaimExtractor extractor)
		{
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._authorizationService = authorizationService;
			this._permissionPolicyService = permissionPolicyService;
			this._extractor = extractor;
		}

		public Boolean HasAuthenticated()
		{
			return this._currentPrincipalResolverService.CurrentPrincipal() != null;
		}

		public String CurrentUser()
		{
			String currentUser = this._extractor.SubjectString(this._currentPrincipalResolverService.CurrentPrincipal());
			return currentUser;
		}

		public async Task<Boolean> HasPermission(params String[] permissions)
		{
			return await this._authorizationService.Authorize(permissions);
		}

		public Task<List<String>> DatasetRolesOf()
		{
			List<String> accesses = this._extractor.DatasetGrants(this._currentPrincipalResolverService.CurrentPrincipal())?.Select(x => x.Access)?.Distinct()?.ToList();
			return Task.FromResult(accesses ?? Enumerable.Empty<String>().ToList());
		}

		public Task<List<String>> AffiliatedDatasetGroupCodes()
		{
			List<String> groups = this._extractor.DatasetGrants(this._currentPrincipalResolverService.CurrentPrincipal())?.Where(x => x.Type == Common.Auth.DatasetGrant.TargetType.Group)?.Select(x => x.Code)?.Distinct()?.ToList();
			return Task.FromResult(groups ?? Enumerable.Empty<String>().ToList());
		}

		public Task<List<Guid>> AffiliatedDatasetIds()
		{
			List<Guid> datasetIds = this._extractor.DatasetGrants(this._currentPrincipalResolverService.CurrentPrincipal())?.Where(x => x.Type == Common.Auth.DatasetGrant.TargetType.Dataset)?
				.Select(x => { return Guid.TryParse(x.Code, out Guid parsed) ? (Guid?)parsed : null; })?
				.Where(x => x.HasValue).Select(x => x.Value)?.Distinct()?.ToList();
			return Task.FromResult(datasetIds ?? Enumerable.Empty<Guid>().ToList());
		}
	}
}
