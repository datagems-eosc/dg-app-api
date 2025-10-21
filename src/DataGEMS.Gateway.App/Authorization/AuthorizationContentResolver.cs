using Cite.Tools.Auth.Claims;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Common.Auth;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.AAI;

namespace DataGEMS.Gateway.App.Authorization
{
    public class AuthorizationContentResolver : IAuthorizationContentResolver
	{
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly IAuthorizationService _authorizationService;
		private readonly ClaimExtractor _extractor;
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly QueryFactory _queryFactory;
		private readonly IAAIService _aaiService;

		public AuthorizationContentResolver(
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			IAuthorizationService authorizationService,
			IPermissionPolicyService permissionPolicyService,
			IAAIService aaiService,
			QueryFactory queryFactory,
			ClaimExtractor extractor)
		{
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._authorizationService = authorizationService;
			this._permissionPolicyService = permissionPolicyService;
			this._queryFactory = queryFactory;
			this._aaiService = aaiService;
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

		public async Task<Guid?> CurrentUserId()
		{
			String currentUser = this.CurrentUser();
			if (String.IsNullOrEmpty(currentUser)) return null;
			Guid userId = await this._queryFactory.Query<UserQuery>().IdpSubjectIds(currentUser).DisableTracking().FirstAsync(x=> x.Id);
			if(userId == default(Guid)) return null;
			return userId;
		}

		public async Task<String> SubjectIdOfCurrentUser()
		{
			Guid? currentUserId = await this.CurrentUserId();
			return await this.SubjectIdOfUserId(currentUserId);
		}

		public async Task<String> SubjectIdOfUserId(Guid? userId)
		{
			if(!userId.HasValue) return null;
			String subjectId = await this._queryFactory.Query<UserQuery>().Ids(userId.Value).DisableTracking().FirstAsync(x => x.IdpSubjectId);
			if (String.IsNullOrEmpty(subjectId)) return null;
			return subjectId;
		}

		public async Task<Boolean> HasPermission(params String[] permissions)
		{
			return await this._authorizationService.Authorize(permissions);
		}

		public ISet<String> PermissionsOfContextRoles(IEnumerable<String> roles)
		{
			return this._permissionPolicyService.PermissionsOfContext(roles);
		}

		public async Task<HashSet<String>> ContextRolesForCollection(Guid collectionId)
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return new HashSet<string>();

			return await this.ContextRolesForCollection(subjectId, collectionId);
		}

		public async Task<HashSet<String>> ContextRolesForCollection(String subjectId, Guid collectionId)
		{
			Dictionary<Guid, HashSet<String>> rolesByCollection= await this.ContextRolesForCollection(subjectId, [collectionId]);
			if(rolesByCollection == null || !rolesByCollection.ContainsKey(collectionId)) return Enumerable.Empty<String>().ToHashSet();
			return rolesByCollection[collectionId];
		}

		public async Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollection(IEnumerable<Guid> collectionIds)
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return new Dictionary<Guid, HashSet<string>>();

			return await this.ContextRolesForCollection(subjectId, collectionIds);
		}

		public async Task<Dictionary<Guid, HashSet<String>>> ContextRolesForCollection(String subjectId, IEnumerable<Guid> collectionIds)
		{
			if (collectionIds == null || !collectionIds.Any()) return new Dictionary<Guid, HashSet<string>>();

			HashSet<Guid> collectionIdsMap = collectionIds.ToHashSet();

			List<ContextGrant> grants = await this._aaiService.LookupUserEffectiveContextGrants(subjectId);
			Dictionary<Guid, HashSet<String>> rolesByCollection = grants
				.Where(x => x.TargetType == ContextGrant.TargetKind.Collection && collectionIdsMap.Contains(x.TargetId))
				.ToDictionaryOfList(x => x.TargetId)
				.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Role).ToHashSet());
			return rolesByCollection;
		}

		public async Task<HashSet<String>> EffectiveContextRolesForDataset(Guid datasetId)
		{
			Dictionary<Guid, HashSet<String>> rolesByDataset = await this.EffectiveContextRolesForDataset([datasetId]);
			if (rolesByDataset == null || !rolesByDataset.ContainsKey(datasetId)) return Enumerable.Empty<String>().ToHashSet();
			return rolesByDataset[datasetId];
		}

		public async Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDataset(IEnumerable<Guid> datasetIds)
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return new Dictionary<Guid, HashSet<string>>();

			return await this.EffectiveContextRolesForDataset(subjectId, datasetIds);
		}

		public async Task<Dictionary<Guid, HashSet<String>>> EffectiveContextRolesForDataset(String subjectId, IEnumerable<Guid> datasetIds)
		{
			if (datasetIds == null || !datasetIds.Any()) return new Dictionary<Guid, HashSet<string>>();

			HashSet<Guid> datasetIdsMap = datasetIds.ToHashSet();
			List<ContextGrant> grants = await this._aaiService.LookupUserEffectiveContextGrants(subjectId);
			Dictionary<Guid, HashSet<String>> rolesByDataset = grants
				.Where(x => x.TargetType == ContextGrant.TargetKind.Dataset && datasetIdsMap.Contains(x.TargetId))
				.ToDictionaryOfList(x => x.TargetId)
				.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Role).ToHashSet());

			return rolesByDataset;
		}

		public async Task<List<String>> ContextRolesOf()
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return Enumerable.Empty<String>().ToList();

			List<ContextGrant> grants = await this._aaiService.LookupUserEffectiveContextGrants(subjectId);
			List<String> accesses = grants.Select(x => x.Role).Distinct().ToList();

			return accesses;
		}

		public async Task<List<Guid>> ContextAffiliatedCollections(String permission)
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return Enumerable.Empty<Guid>().ToList();

			ISet<String> contextRolesWithPermission = this._permissionPolicyService.ContextRolesHaving(permission);
			if(contextRolesWithPermission == null || contextRolesWithPermission.Count==0) return Enumerable.Empty<Guid>().ToList();

			List<ContextGrant> grants = await this._aaiService.LookupUserEffectiveContextGrants(subjectId);
			List<Guid> collectionIds = grants.Where(x => x.TargetType == ContextGrant.TargetKind.Collection && contextRolesWithPermission.Contains(x.Role)).Select(x => x.TargetId).Distinct().ToList();

			return collectionIds;
		}

		public async Task<List<Guid>> EffectiveContextAffiliatedDatasets(String permission)
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return Enumerable.Empty<Guid>().ToList();

			ISet<String> contextRolesWithPermission = this._permissionPolicyService.ContextRolesHaving(permission);
			if (contextRolesWithPermission == null || contextRolesWithPermission.Count == 0) return Enumerable.Empty<Guid>().ToList();

			List<ContextGrant> grants = await this._aaiService.LookupUserEffectiveContextGrants(subjectId);
			List<Guid> datasetIds = grants.Where(x => x.TargetType == ContextGrant.TargetKind.Dataset && contextRolesWithPermission.Contains(x.Role)).Select(x => x.TargetId).Distinct().ToList();

			return datasetIds;
		}
	}
}
