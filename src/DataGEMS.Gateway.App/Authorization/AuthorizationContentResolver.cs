using Cite.Tools.Auth.Claims;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Common.Auth;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.AAI;
using DataGEMS.Gateway.App.Service.DataManagement.Model;

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

		private async Task<Dictionary<Guid, HashSet<String>>> ContextRolesForDataset(String subjectId, IEnumerable<Guid> datasetIds)
		{
			if (datasetIds == null || !datasetIds.Any()) return new Dictionary<Guid, HashSet<string>>();

			HashSet<String> datasetIdsAsString = datasetIds.Select(x => x.ToString()).ToHashSet();

			List<ContextGrant> grants = await this._aaiService.UserContextGrants(subjectId);
			grants = grants.Where(x => x.Type == ContextGrant.TargetType.Dataset && datasetIdsAsString.Contains(x.Code)).ToList();

			Dictionary<String, List<ContextGrant>> grantsByCode = grants.ToDictionaryOfList(x => x.Code);
			Dictionary<Guid, HashSet<String>> rolesByDataset = grantsByCode
				.Where(x => Guid.TryParse(x.Key, out Guid _))
				.Select(x => new { Key = Guid.Parse(x.Key), Value = x.Value.Select(x => x.Access).ToHashSet() })
				.ToDictionary(x => x.Key, x => x.Value);
			return rolesByDataset;
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

			HashSet<String> collectionIdsAsString = collectionIds.Select(x => x.ToString()).ToHashSet();

			List<ContextGrant> grants = await this._aaiService.UserContextGrants(subjectId);
			grants = grants.Where(x => x.Type == ContextGrant.TargetType.Group && collectionIdsAsString.Contains(x.Code)).ToList();

			Dictionary<String, List<ContextGrant>> grantsByCode = grants.ToDictionaryOfList(x => x.Code);
			Dictionary<Guid, HashSet<String>> rolesByCollection = grantsByCode
				.Where(x => Guid.TryParse(x.Key, out Guid _))
				.Select(x => new { Key = Guid.Parse(x.Key), Value = x.Value.Select(x => x.Access).ToHashSet() })
				.ToDictionary(x => x.Key, x => x.Value);
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

			Dictionary<Guid, HashSet<String>> directRoles = await this.ContextRolesForDataset(subjectId, datasetIds);

			List<DatasetCollection> datasetCollections = await this._queryFactory.Query<DatasetCollectionLocalQuery>()
				.Authorize(AuthorizationFlags.None)
				.DatasetIds(datasetIds)
				.CollectAsyncAsModels();
			Dictionary<Guid, HashSet<Guid>> collectionsOfDatasetMap = datasetCollections
				.ToDictionaryOfList(x => x.DatasetId)
				.Select(x => new { DatasetId = x.Key, CollectionIds = x.Value.Select(y => y.CollectionId).ToHashSet() })
				.ToDictionary(x => x.DatasetId, x => x.CollectionIds);

			List<Guid> collectionIds = collectionsOfDatasetMap.SelectMany(x => x.Value).Distinct().ToList();
			Dictionary<Guid, HashSet<String>> collectionRoles = await this.ContextRolesForCollection(subjectId, collectionIds);

			Dictionary<Guid, HashSet<String>> datasetRoles = new Dictionary<Guid, HashSet<string>>();
			foreach (Guid datasetId in datasetIds.Distinct().ToList())
			{
				HashSet<String> roles = new HashSet<string>();
				if (directRoles.ContainsKey(datasetId)) roles.AddRange(directRoles[datasetId]);
				if (collectionsOfDatasetMap.ContainsKey(datasetId)) roles.AddRange(collectionsOfDatasetMap[datasetId]
					.Where(x => collectionRoles.ContainsKey(x))
					.SelectMany(x => collectionRoles[x])
					.ToHashSet());
				datasetRoles[datasetId] = roles;
			}
			return datasetRoles;
		}

		public async Task<List<String>> ContextRolesOf()
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return Enumerable.Empty<String>().ToList();

			List<ContextGrant> grants = await this._aaiService.UserContextGrants(subjectId);
			List<String> accesses = grants.Select(x => x.Access).Distinct().ToList();

			return accesses;
		}

		public async Task<List<Guid>> ContextAffiliatedCollections(String permission)
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return Enumerable.Empty<Guid>().ToList();

			ISet<String> contextRolesWithPermission = this._permissionPolicyService.ContextRolesHaving(permission);
			if(contextRolesWithPermission == null || contextRolesWithPermission.Count==0) return Enumerable.Empty<Guid>().ToList();

			List<ContextGrant> grants = await this._aaiService.UserContextGrants(subjectId);
			List<String> groups = grants.Where(x => x.Type == ContextGrant.TargetType.Group && contextRolesWithPermission.Contains(x.Access)).Select(x => x.Code).ToList();

			List<Guid> groupIds = groups.Select(x => Guid.TryParse(x, out Guid parsed) ? (Guid?)parsed : null).Where(x => x.HasValue).Select(x => x.Value).ToList();

			return groupIds;
		}

		public async Task<List<Guid>> EffectiveContextAffiliatedDatasets(String permission)
		{
			String subjectId = await this.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return Enumerable.Empty<Guid>().ToList();

			ISet<String> contextRolesWithPermission = this._permissionPolicyService.ContextRolesHaving(permission);
			if (contextRolesWithPermission == null || contextRolesWithPermission.Count == 0) return Enumerable.Empty<Guid>().ToList();

			List<ContextGrant> grants = await this._aaiService.UserContextGrants(subjectId);
			List<String> directDatasets = grants.Where(x => x.Type == ContextGrant.TargetType.Dataset && contextRolesWithPermission.Contains(x.Access)).Select(x => x.Code).ToList();
			List<Guid> directDatasetIds = directDatasets.Select(x => Guid.TryParse(x, out Guid parsed) ? (Guid?)parsed : null).Where(x => x.HasValue).Select(x => x.Value).ToList();

			List<String> collections = grants.Where(x => x.Type == ContextGrant.TargetType.Group && contextRolesWithPermission.Contains(x.Access)).Select(x => x.Code).ToList();
			List<Guid> collectionIds = collections.Select(x => Guid.TryParse(x, out Guid parsed) ? (Guid?)parsed : null).Where(x => x.HasValue).Select(x => x.Value).ToList();
			List<DatasetCollection> datasetCollections = await this._queryFactory.Query<DatasetCollectionLocalQuery>()
				.Authorize(AuthorizationFlags.None)
				.CollectionIds(collectionIds)
				.CollectAsyncAsModels();
			List<Guid> collectionDatasetIds = datasetCollections.Select(x => x.DatasetId).ToList();

			List<Guid> datasetIds = directDatasetIds.Concat(collectionDatasetIds).Distinct().ToList();

			return datasetIds;
		}
	}
}
