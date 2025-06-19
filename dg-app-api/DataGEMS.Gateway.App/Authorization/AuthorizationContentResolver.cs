using Cite.Tools.Auth.Claims;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Query;
using System.Collections.Generic;

namespace DataGEMS.Gateway.App.Authorization
{
	public class AuthorizationContentResolver : IAuthorizationContentResolver
	{
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly IAuthorizationService _authorizationService;
		private readonly ClaimExtractor _extractor;
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly QueryFactory _queryFactory;

		public AuthorizationContentResolver(
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			IAuthorizationService authorizationService,
			IPermissionPolicyService permissionPolicyService,
			QueryFactory queryFactory,
			ClaimExtractor extractor)
		{
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._authorizationService = authorizationService;
			this._permissionPolicyService = permissionPolicyService;
			this._queryFactory = queryFactory;
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

		public ISet<String> PermissionsOfDatasetRoles(IEnumerable<String> datasetRoles)
		{
			return this._permissionPolicyService.PermissionsOfDataset(datasetRoles);
		}

		public async Task<Dictionary<Guid, HashSet<String>>> DatasetRolesForDataset(IEnumerable<Guid> datasetIds)
		{
			if (datasetIds == null || !datasetIds.Any()) return new Dictionary<Guid, HashSet<string>>();

			List<Common.Auth.DatasetGrant> grants = this._extractor.DatasetGrants(this._currentPrincipalResolverService.CurrentPrincipal());
			if (grants == null || grants.Count == 0) return new Dictionary<Guid, HashSet<string>>();

			List<DataManagement.Model.DatasetCollection> datasetCollections = await this._queryFactory.Query<DatasetCollectionLocalQuery>()
				.Authorize(AuthorizationFlags.Any)
				.DatasetIds(datasetIds)
				.CollectAsyncAsModels();
			List<DataManagement.Model.Collection> collections = await this._queryFactory.Query<CollectionLocalQuery>()
				.Authorize(AuthorizationFlags.Any)
				.Ids(datasetCollections.Select(x => x.CollectionId).Distinct().ToList())
				.CollectAsyncAsModels();
			Dictionary<Guid, String> codeOfCollectionId = collections.ToDictionary(x => x.Id, x => x.Code);

			Dictionary<Guid, List<String>> collectionCodesOfDataset = datasetCollections
				.ToDictionaryOfList(x => x.DatasetId)
				.ToDictionary(x => x.Key, x => x.Value
					.Select(y => y.CollectionId)
					.Distinct()
					.Where(y => codeOfCollectionId.ContainsKey(y))
					.Select(y => codeOfCollectionId[y])
					.ToList());

			Dictionary<Guid, HashSet<String>> rolesByDataset = new Dictionary<Guid, HashSet<string>>();
			foreach (Guid datasetId in datasetIds.Distinct().ToList())
			{
				List<String> accesses = grants.Where(x =>
						(x.Type == Common.Auth.DatasetGrant.TargetType.Dataset && String.Equals(x.Code, datasetId.ToString(), StringComparison.OrdinalIgnoreCase)) ||
						(x.Type == Common.Auth.DatasetGrant.TargetType.Group && collectionCodesOfDataset.ContainsKey(datasetId) && collectionCodesOfDataset[datasetId].Contains(x.Code))
					).Select(x => x.Access).ToList();
				rolesByDataset.Add(datasetId, accesses.ToHashSet());
			}
			return rolesByDataset;
		}

		public async Task<Dictionary<Guid, HashSet<String>>> DatasetRolesForCollection(IEnumerable<Guid> collectionIds)
		{
			if (collectionIds == null || !collectionIds.Any()) return new Dictionary<Guid, HashSet<string>>();

			List<Common.Auth.DatasetGrant> grants = this._extractor.DatasetGrants(this._currentPrincipalResolverService.CurrentPrincipal());
			if (grants == null || grants.Count == 0) return new Dictionary<Guid, HashSet<string>>();

			List<DataManagement.Model.Collection> collections = await this._queryFactory.Query<CollectionLocalQuery>()
				.Authorize(AuthorizationFlags.Any)
				.Ids(collectionIds.Distinct().ToList())
				.CollectAsyncAsModels();
			Dictionary<Guid, String> codeOfCollectionId = collections.ToDictionary(x => x.Id, x => x.Code);

			Dictionary<Guid, HashSet<String>> rolesByCollection = new Dictionary<Guid, HashSet<string>>();
			foreach (Guid collectionId in collectionIds.Distinct().ToList())
			{
				List<String> accesses = grants.Where(x =>
						x.Type == Common.Auth.DatasetGrant.TargetType.Group && codeOfCollectionId.ContainsKey(collectionId) && codeOfCollectionId[collectionId].Equals(x.Code)
					).Select(x => x.Access).ToList();
				rolesByCollection.Add(collectionId, accesses.ToHashSet());
			}
			return rolesByCollection;
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
