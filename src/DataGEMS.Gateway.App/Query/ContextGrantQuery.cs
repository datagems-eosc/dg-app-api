using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Auth;
using DataGEMS.Gateway.App.Service.AAI;
using System.Data;

namespace DataGEMS.Gateway.App.Query
{
	public class ContextGrantQuery : Cite.Tools.Data.Query.IQuery
	{
		private List<Guid> _datasetIds { get; set; }
		private List<Guid> _collectionIds { get; set; }
		private List<String> _roles { get; set; }
		private String _subjectId { get; set; }
		private List<ContextGrant.TargetKind> _targetKinds { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IAAIService _aaiService;
		private readonly App.Authorization.IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public ContextGrantQuery(
			IAAIService aaiService,
			App.Authorization.IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._aaiService = aaiService;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public ContextGrantQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = datasetIds?.ToList(); return this; }
		public ContextGrantQuery DatasetIds(Guid datasetId) { this._datasetIds = datasetId.AsList(); return this; }
		public ContextGrantQuery CollectionIds(IEnumerable<Guid> collectionIds) { this._collectionIds = collectionIds?.ToList(); return this; }
		public ContextGrantQuery CollectionIds(Guid collectionId) { this._collectionIds = collectionId.AsList(); return this; }
		public ContextGrantQuery Roles(IEnumerable<String> roles) { this._roles = roles?.ToList(); return this; }
		public ContextGrantQuery Roles(String role) { this._roles = role.AsList(); return this; }
		public ContextGrantQuery Subject(String subjectId) { this._subjectId = subjectId; return this; }
		public ContextGrantQuery TargetKinds(IEnumerable<ContextGrant.TargetKind> targetKinds) { this._targetKinds = targetKinds?.ToList(); return this; }
		public ContextGrantQuery TargetKinds(ContextGrant.TargetKind targetKind) { this._targetKinds = targetKind.AsList(); return this; }

		protected bool IsFalseQuery()
		{
			return this._datasetIds.IsNotNullButEmpty() || this._collectionIds.IsNotNullButEmpty() || this._roles.IsNotNullButEmpty() || this._targetKinds.IsNotNullButEmpty() || String.IsNullOrEmpty(this._subjectId);
		}

		public async Task<List<App.Common.Auth.ContextGrant>> CollectAsync()
		{
			String subjectId = await this._authorizationContentResolver.SubjectIdOfCurrentUser();
			if (String.IsNullOrEmpty(subjectId)) return new List<Common.Auth.ContextGrant>();
			Boolean isOwned = String.IsNullOrEmpty(this._subjectId) || subjectId.Equals(this._subjectId, StringComparison.OrdinalIgnoreCase);

			List<App.Common.Auth.ContextGrant> grants = null;
			if(isOwned) grants = await this._aaiService.LookupUserContextGrants(subjectId);
			else
			{
				Boolean authorized = await this._authorizationService.Authorize(Permission.LookupContextGrantOther);
				if(!authorized) return new List<Common.Auth.ContextGrant>();
				grants = await this._aaiService.LookupUserContextGrants(this._subjectId);
			}

			if (this._datasetIds != null) grants = grants.Where(x => x.TargetType == ContextGrant.TargetKind.Dataset && this._datasetIds.Contains(x.TargetId)).ToList();
			if (this._collectionIds != null) grants = grants.Where(x => x.TargetType == ContextGrant.TargetKind.Collection && this._collectionIds.Contains(x.TargetId)).ToList();
			if (this._targetKinds != null) grants = grants.Where(x => this._targetKinds.Contains(x.TargetType)).ToList();
			if (this._roles != null)
			{
				HashSet<string> rolesToUse = this._roles.Select(x => x.ToLowerInvariant()).ToHashSet();
				grants = grants.Where(x => rolesToUse.Contains(x.Role.ToLowerInvariant())).ToList();
			}
			return grants;
		}
	}
}
