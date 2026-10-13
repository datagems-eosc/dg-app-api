using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Auth;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;
using static DataGEMS.Gateway.App.Common.Auth.DatasetContextGrants;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public class AAIKeycloakService : IAAIService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly AAIConfig _config;
		private readonly ErrorThesaurus _errors;
		private readonly ILogger<AAIKeycloakService> _logger;
		private readonly AAICache _aaiCache;
		private readonly EventBroker _eventBroker;

		public AAIKeycloakService(
			AAIConfig config,
			ErrorThesaurus errors,
			AAICache aaiCache,
			EventBroker eventBroker,
			IHttpClientFactory httpClientFactory,
			IAccessTokenService accessTokenService,
			JsonHandlingService jsonHandlingService,
			LogCorrelationScope logCorrelationScope,
			ILogger<AAIKeycloakService> logger)
		{
			this._config = config;
			this._errors = errors;
			this._logger = logger;
			this._aaiCache = aaiCache;
			this._httpClientFactory = httpClientFactory;
			this._eventBroker = eventBroker;
			this._logCorrelationScope = logCorrelationScope;
			this._accessTokenService = accessTokenService;
			this._jsonHandlingService = jsonHandlingService;
		}

		private async Task<Model.Group> FindUserPrincipalGroup(String subjectlId)
		{
			String subjectIdToUse = subjectlId.ToLowerInvariant();

			Model.Group cached = await this._aaiCache.UserPrincipalGroupLookup(subjectIdToUse);
			if (cached != null) return cached;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage lookupSubjectHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupsEndpoint}?search={subjectIdToUse}&briefRepresentation=false");
			lookupSubjectHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupSubjectHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String groupsContent = await this.SendRequest(lookupSubjectHttpRequest);
			List<Model.Group> groups = null;
			try { groups = String.IsNullOrEmpty(groupsContent) ? new List<Model.Group>() : this._jsonHandlingService.FromJson<List<Model.Group>>(groupsContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			groups = groups.SelectMany(x => x.SubGroups).Where(x => x.Path.StartsWith($"/{this._config.ContextGrantGroupPrefix}/{subjectIdToUse}")).ToList();

			if (groups.Count == 0) return null;
			else if (groups.Count > 1) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);

			await this._aaiCache.UserPrincipalGroupUpdate(subjectIdToUse, groups[0]);
			return groups[0];
		}

		private async Task<Model.Group> FindUserGroupPrincipalGroup(String groupId)
		{
			String groupIdToUse = groupId.ToLowerInvariant();

			Model.Group cached = await this._aaiCache.UserGroupPrincipalGroupLookup(groupIdToUse);
			if (cached != null) return cached;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage lookupSubjectHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupEndpoint.Replace("{groupId}", groupIdToUse)}");
			lookupSubjectHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupSubjectHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String groupsContent = await this.SendRequest(lookupSubjectHttpRequest);
			Model.Group group = null;
			try { group = String.IsNullOrEmpty(groupsContent) ? null : this._jsonHandlingService.FromJson<Model.Group>(groupsContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			if (group == null) return null;

			await this._aaiCache.UserGroupPrincipalGroupUpdate(groupIdToUse, group);
			return group;
		}

		private async Task<List<Model.Group>> FindSubGroups(String parentId)
		{
			List<Model.Group> cached = await this._aaiCache.SubGroupsLookup(parentId);
			if (cached != null) return cached;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			List<Service.AAI.Model.Group> children = new List<Service.AAI.Model.Group>();
			int first = 0;
			int max = 100;

			while (true)
			{
				HttpRequestMessage lookupChildrenHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupChildrenEndpoint.Replace("{groupId}", parentId)}?first={first}&max={max}");
				lookupChildrenHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
				lookupChildrenHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				String childrenContent = await this.SendRequest(lookupChildrenHttpRequest);
				List<Model.Group> pageChildren = null;
				try { pageChildren = String.IsNullOrEmpty(childrenContent) ? new List<Model.Group>() : this._jsonHandlingService.FromJson<List<Model.Group>>(childrenContent); }
				catch (System.Exception ex)
				{
					this._logger.LogError(ex, "Failed to parse response: {content}", childrenContent);
					throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
				}
				children.AddRange(pageChildren);
				if (pageChildren.Count < max) break;
				first += max;
			}

			await this._aaiCache.SubGroupsUpdate(parentId, children);
			return children;
		}

		private async Task<List<Model.Group>> FindUserGroups(String userSubjectId)
		{
			String userSubjectIdToUse = userSubjectId.ToLowerInvariant();

			List<Model.Group> cached = await this._aaiCache.UserGroupsLookup(userSubjectIdToUse);
			if (cached != null) return cached;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			List<Service.AAI.Model.Group> groups = new List<Service.AAI.Model.Group>();
			int first = 0;
			int max = 100;

			while (true)
			{
				HttpRequestMessage lookupSubjectGroupsHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.UserGroupsEndpoint.Replace("{userId}", userSubjectIdToUse)}?briefRepresentation=false&first={first}&max={max}");
				lookupSubjectGroupsHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
				lookupSubjectGroupsHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				String groupsContent = await this.SendRequest(lookupSubjectGroupsHttpRequest);
				List<Model.Group> pageGroups = null;
				try { pageGroups = String.IsNullOrEmpty(groupsContent) ? new List<Model.Group>() : this._jsonHandlingService.FromJson<List<Model.Group>>(groupsContent); }
				catch (System.Exception ex)
				{
					this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
					throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
				}
				List<Model.Group> filteredGroups = pageGroups.Where(x => x.Path.StartsWith($"/{this._config.ContextGrantGroupPrefix}") && x.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length == 2).ToList();
				groups.AddRange(filteredGroups);
				if (pageGroups.Count < max) break;
				first += max;
			}

			await this._aaiCache.UserGroupsUpdate(userSubjectIdToUse, groups);
			return groups;
		}

		public async Task AddUserToGroup(String userSubjectId, String userGroupId)
		{
			String userSubjectIdToUse = userSubjectId.ToLowerInvariant();

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage addMembershipGroupsHttpRequest = new HttpRequestMessage(HttpMethod.Put, $"{this._config.BaseUrl}{this._config.UserGroupMembershipEndpoint.Replace("{userId}", userSubjectIdToUse).Replace("{groupId}", userGroupId)}");
			addMembershipGroupsHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			addMembershipGroupsHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			await this.SendRequest(addMembershipGroupsHttpRequest);
			this._eventBroker.EmitHierarchyContextGrantTouched([userSubjectIdToUse, userGroupId]);
		}

		public async Task BootstrapUserContextGrants(String userSubjectId)
		{
			String userSubjectIdToUse = userSubjectId.ToLowerInvariant();

			Model.Group principalGroup = await this.FindUserPrincipalGroup(userSubjectIdToUse);
			if (principalGroup != null) return;

			String topLevel = await this.EnsureHierarchyLevel(null, this._config.ContextGrantGroupPrefix, null);
			String principalLevel = await this.EnsureHierarchyLevel(topLevel, userSubjectIdToUse, new Dictionary<string, List<string>>() { { this._config.ContextGrantTypeAttributeName, [this._config.ContextGrantTypeUserAttributeValue] } });

			await this.AddUserToGroup(userSubjectId, principalLevel);
			this._eventBroker.EmitHierarchyContextGrantTouched([userSubjectIdToUse, principalLevel, topLevel]);
		}

		private async Task<String> EnsureHierarchyLevel(String parentId, String name, Dictionary<string, List<string>> attributes)
		{
			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			String requestUrl = String.IsNullOrEmpty(parentId) ? 
				$"{this._config.BaseUrl}{this._config.GroupsEndpoint}" : 
				$"{this._config.BaseUrl}{this._config.GroupChildrenEndpoint.Replace("{groupId}", parentId)}";

			int first = 0;
			List<Model.Group> matchingGroups = [];

			while (true)
			{
				String separator = requestUrl.Contains('?') ? "&" : "?";
				using HttpRequestMessage lookupHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{requestUrl}{separator}first={first}&max={this._config.QueryingPaginationSize}");
				lookupHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
				lookupHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				String groupsContent = await this.SendRequest(lookupHttpRequest);
				List<Model.Group> groups;
				try { groups = String.IsNullOrEmpty(groupsContent) ? new List<Model.Group>() : this._jsonHandlingService.FromJson<List<Model.Group>>(groupsContent); }
				catch (System.Exception ex)
				{
					this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
					throw new DGUnderpinningException( this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
				}
				matchingGroups.AddRange(groups.Where(x => x.Name == name));
				if (matchingGroups.Count > 1) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
				if (groups.Count < this._config.QueryingPaginationSize) break;
				first += this._config.QueryingPaginationSize;
			}

			if (matchingGroups.Count == 1) return matchingGroups[0].Id;
			Model.Group create = new Model.Group() { Name = name, Attributes = attributes };

			using HttpRequestMessage createHttpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(create), Encoding.UTF8, "application/json")
			};
			createHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			createHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String content = await this.SendRequest(createHttpRequest, true);
			String[] locationParts = content.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			if (locationParts.Length == 0) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			return locationParts.Last();
		}

		private async Task EnsureRole(String groupId, String roleName)
		{
			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage lookupRoleHttpRequest = null;
			lookupRoleHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.RolesEndpoint.Replace("{roleName}", roleName)}");
			lookupRoleHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupRoleHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String rolesContent = await this.SendRequest(lookupRoleHttpRequest);
			Model.RoleMapping roles = null;
			try { roles = String.IsNullOrEmpty(rolesContent) ? null : this._jsonHandlingService.FromJson<Model.RoleMapping>(rolesContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", rolesContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			if(roles == null) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);

			HttpRequestMessage addRoleHttpRequest = null;
			addRoleHttpRequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.GroupRoleMappingsEndpoint.Replace("{groupId}", groupId)}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(new Model.RoleMapping[] { roles }), Encoding.UTF8, "application/json")
			};
			addRoleHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			addRoleHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			await this.SendRequest(addRoleHttpRequest);
		}

		private async Task RemoveRole(String groupId, String roleName)
		{
			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage lookupRoleHttpRequest = null;
			lookupRoleHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.RolesEndpoint.Replace("{roleName}", roleName)}");
			lookupRoleHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupRoleHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String rolesContent = await this.SendRequest(lookupRoleHttpRequest);
			Model.RoleMapping roles = null;
			try { roles = String.IsNullOrEmpty(rolesContent) ? null : this._jsonHandlingService.FromJson<Model.RoleMapping>(rolesContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", rolesContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			if (roles == null) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);

			HttpRequestMessage removeRoleHttpRequest = null;
			removeRoleHttpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{this._config.BaseUrl}{this._config.GroupRoleMappingsEndpoint.Replace("{groupId}", groupId)}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(new Model.RoleMapping[] { roles }), Encoding.UTF8, "application/json")
			};
			removeRoleHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			removeRoleHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			await this.SendRequest(removeRoleHttpRequest);
		}

		private List<ContextGrant> ConvertToContextGrant(String principalId, Model.Group princialGroup, Model.Group targetGroup)
		{
			if (!Guid.TryParse(targetGroup.Name, out Guid targetId)) return null;

			if (targetGroup.Attributes == null ||
				!targetGroup.Attributes.ContainsKey(this._config.ContextGrantTypeAttributeName) ||
				targetGroup.Attributes[this._config.ContextGrantTypeAttributeName] == null ||
				targetGroup.Attributes[this._config.ContextGrantTypeAttributeName].Count == 0) return null;

			ContextGrant.PrincipalKind principalType;
			if (princialGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeUserAttributeValue, StringComparison.OrdinalIgnoreCase))) principalType = ContextGrant.PrincipalKind.User;
			else if (princialGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeGroupAttributeValue, StringComparison.OrdinalIgnoreCase))) principalType = ContextGrant.PrincipalKind.Group;
			else return null;

			ContextGrant.TargetKind targetType;
			if (targetGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeDatasetAttributeValue, StringComparison.OrdinalIgnoreCase))) targetType = ContextGrant.TargetKind.Dataset;
			else if (targetGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeCollectionAttributeValue, StringComparison.OrdinalIgnoreCase))) targetType = ContextGrant.TargetKind.Collection;
			else return null;

			if (targetGroup.RealmRoles == null || targetGroup.RealmRoles.Count == 0) return null;

			List<ContextGrant> targetGrants = targetGroup.RealmRoles.Select(x => new ContextGrant()
			{
				PrincipalId = principalId,
				PrincipalType = principalType,
				TargetType = targetType,
				TargetId = targetId,
				Role = x
			}).ToList();
			return targetGrants;
		}

		public async Task<List<ContextGrant>> LookupUserGroupContextGrants(String userGroupId)
		{
			String userGroupIdIdToUse = userGroupId.ToLowerInvariant();

			Model.Group principalGroup = await this.FindUserGroupPrincipalGroup(userGroupIdIdToUse);
			if (principalGroup == null) return new List<ContextGrant>();

			List<Model.Group> children = await this.FindSubGroups(principalGroup.Id);
			if (children == null || children.Count == 0) return new List<ContextGrant>();

			List<ContextGrant> grants = new List<ContextGrant>();

			foreach (Model.Group child in children)
			{
				List<ContextGrant> targetGrants = this.ConvertToContextGrant(userGroupId, principalGroup, child);
				if (targetGrants == null) continue;
				grants.AddRange(targetGrants);
			}
			return grants;
		}

		public async Task<List<ContextGrant>> LookupUserContextGrants(String userSubjectId)
		{
			String userSubjectIdToUse = userSubjectId.ToLowerInvariant();

			List<Model.Group> groups = await this.FindUserGroups(userSubjectIdToUse);
			if (groups == null || groups.Count == 0) return new List<ContextGrant>();

			List<ContextGrant> grants = new List<ContextGrant>();
			foreach (Model.Group group in groups)
			{
				List<Model.Group> children = await this.FindSubGroups(group.Id);
				if (children == null || children.Count == 0) continue;

				foreach (Model.Group child in children)
				{
					List<ContextGrant> targetGrants = this.ConvertToContextGrant(userSubjectId, group, child);
					if (targetGrants == null) continue;
					grants.AddRange(targetGrants);
				}
			}
			return grants;
		}

		public async Task AssignCollectionGrantToUser(String subjectId, Guid collectionId, String role)
		{
			await this.AssignTargetGrantToUser(subjectId, collectionId, this._config.ContextGrantTypeCollectionAttributeValue, [role]);
		}

		public async Task AssignCollectionGrantToUser(String subjectId, Guid collectionId, List<String> roles)
		{
			await this.AssignTargetGrantToUser(subjectId, collectionId, this._config.ContextGrantTypeCollectionAttributeValue, roles);
		}

		public async Task AssignDatasetGrantToUser(String subjectId, Guid datasetId, String role)
		{
			await this.AssignTargetGrantToUser(subjectId, datasetId, this._config.ContextGrantTypeDatasetAttributeValue, [role]);
		}

		public async Task AssignDatasetGrantToUser(String subjectId, Guid datasetId, List<String> roles)
		{
			await this.AssignTargetGrantToUser(subjectId, datasetId, this._config.ContextGrantTypeDatasetAttributeValue, roles);
		}

		private async Task AssignTargetGrantToUser(String subjectId, Guid targetId, String attributeValue, List<String> roles)
		{
			String subjectIdToUse = subjectId.ToLowerInvariant();

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Model.Group group = await this.FindUserPrincipalGroup(subjectIdToUse);
			if(group == null) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);

			String targetLevel = await this.EnsureHierarchyLevel(group.Id, targetId.ToString().ToLowerInvariant(), new Dictionary<string, List<string>>() { { this._config.ContextGrantTypeAttributeName, [attributeValue] } });

			foreach (String role in roles)
			{
				await EnsureRole(targetLevel, role);
			}
			this._eventBroker.EmitHierarchyContextGrantTouched([subjectIdToUse, targetLevel, group.Id]);
		}

		public async Task AssignCollectionGrantToUserGroup(String groupId, Guid collectionId, String role)
		{
			await this.AssignTargetGrantToUserGroup(groupId, collectionId, this._config.ContextGrantTypeCollectionAttributeValue, [role]);
		}

		public async Task AssignCollectionGrantToUserGroup(String groupId, Guid collectionId, List<String> roles)
		{
			await this.AssignTargetGrantToUserGroup(groupId, collectionId, this._config.ContextGrantTypeCollectionAttributeValue, roles);
		}

		public async Task AssignDatasetGrantToUserGroup(String groupId, Guid datasetId, String role)
		{
			await this.AssignTargetGrantToUserGroup(groupId, datasetId, this._config.ContextGrantTypeDatasetAttributeValue, [role]);
		}

		public async Task AssignDatasetGrantToUserGroup(String groupId, Guid datasetId, List<String> roles)
		{
			await this.AssignTargetGrantToUserGroup(groupId, datasetId, this._config.ContextGrantTypeDatasetAttributeValue, roles);
		}

		private async Task AssignTargetGrantToUserGroup(String groupId, Guid targetId, String attributeValue, List<String> roles)
		{
			String groupIdToUse = groupId.ToLowerInvariant();

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Model.Group group = await this.FindUserGroupPrincipalGroup(groupIdToUse);
			if (group == null) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);

			String targetLevel = await this.EnsureHierarchyLevel(group.Id, targetId.ToString().ToLowerInvariant(), new Dictionary<string, List<string>>() { { this._config.ContextGrantTypeAttributeName, [attributeValue] } });

			foreach (String role in roles)
			{
				await EnsureRole(targetLevel, role);
			}
			this._eventBroker.EmitHierarchyContextGrantTouched([groupIdToUse, targetLevel, group.Id]);
		}

		public async Task UnassignCollectionGrantFromUser(String subjectId, Guid collectionId, String role)
		{
			await this.UnassignTargetGrantFromUser(subjectId, collectionId, this._config.ContextGrantTypeCollectionAttributeValue, role);
		}

		public async Task UnassignDatasetGrantFromUser(String subjectId, Guid datasetId, String role)
		{
			await this.UnassignTargetGrantFromUser(subjectId, datasetId, this._config.ContextGrantTypeDatasetAttributeValue, role);
		}

		private async Task UnassignTargetGrantFromUser(String subjectId, Guid targetId, String attributeValue, String role)
		{
			String subjectIdToUse = subjectId.ToLowerInvariant();

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Model.Group group = await this.FindUserPrincipalGroup(subjectIdToUse);
			if (group == null) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);

			String targetLevel = await this.EnsureHierarchyLevel(group.Id, targetId.ToString().ToLowerInvariant(), new Dictionary<string, List<string>>() { { this._config.ContextGrantTypeAttributeName, [attributeValue] } });

			await RemoveRole(targetLevel, role);
			this._eventBroker.EmitHierarchyContextGrantTouched([subjectIdToUse, targetLevel, group.Id]);
		}

		public async Task UnassignCollectionGrantFromUserGroup(String groupId, Guid collectionId, String role)
		{
			await this.UnassignTargetGrantFromUserGroup(groupId, collectionId, this._config.ContextGrantTypeCollectionAttributeValue, role);
		}

		public async Task UnassignDatasetGrantFromUserGroup(String groupId, Guid datasetId, String role)
		{
			await this.UnassignTargetGrantFromUserGroup(groupId, datasetId, this._config.ContextGrantTypeDatasetAttributeValue, role);
		}

		private async Task UnassignTargetGrantFromUserGroup(String groupId, Guid targetId, String attributeValue, String role)
		{
			String groupIdToUse = groupId.ToLowerInvariant();

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Model.Group group = await this.FindUserGroupPrincipalGroup(groupIdToUse);
			if (group == null) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);

			String targetLevel = await this.EnsureHierarchyLevel(group.Id, targetId.ToString().ToLowerInvariant(), new Dictionary<string, List<string>>() { { this._config.ContextGrantTypeAttributeName, [attributeValue] } });

			await RemoveRole(targetLevel, role);
			this._eventBroker.EmitHierarchyContextGrantTouched([groupIdToUse, targetLevel, group.Id]);
		}

		public async Task DeleteCollectionGrants(Guid collectionId)
		{
			await this.DeleteTargetGrants(collectionId);
		}

		public async Task DeleteDatasetGrants(Guid datasetId)
		{
			await this.DeleteTargetGrants(datasetId);
		}

		private async Task DeleteTargetGrants(Guid targetId)
		{
			String targetIdToUse = targetId.ToString().ToLowerInvariant();

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage lookupTargetHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupsEndpoint}?search={targetIdToUse}");
			lookupTargetHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupTargetHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String groupsContent = await this.SendRequest(lookupTargetHttpRequest);
			List<Model.Group> groups = null;
			try { groups = String.IsNullOrEmpty(groupsContent) ? new List<Model.Group>() : this._jsonHandlingService.FromJson<List<Model.Group>>(groupsContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			groups = groups.SelectMany(x => x.SubGroups.SelectMany(y => y.SubGroups)).Where(x => x.Name.Equals(targetIdToUse, StringComparison.OrdinalIgnoreCase) && x.Path.StartsWith($"/{this._config.ContextGrantGroupPrefix}")).ToList();

			foreach(Model.Group group in groups)
			{
				HttpRequestMessage deleteTargetHttpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{this._config.BaseUrl}{this._config.GroupEndpoint.Replace("{groupId}", group.Id)}");
				deleteTargetHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
				deleteTargetHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				await this.SendRequest(deleteTargetHttpRequest);
			}
			this._eventBroker.EmitHierarchyContextGrantTouched(groups.Select(x=> x.Id).ToList());
		}

		private async Task<string> SendRequest(HttpRequestMessage request, Boolean locationHeaderReturn = false)
		{
			this._logger.Debug("Sending request to {url}", request.RequestUri);
			HttpResponseMessage response = null;
			try { 
				response = await this._httpClientFactory.CreateClient().SendAsync(request);
				this._logger.Debug("Received response with status code {statusCode}", response?.StatusCode);
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}

			try
			{
				if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
				else response.EnsureSuccessStatusCode();
			}
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			String content = await response.Content.ReadAsStringAsync();
			this._logger.Debug("Response content: {content}", content);

			if (locationHeaderReturn) content = response.Headers.Location?.ToString();
			return content;
		}

		/// <summary>
		/// Retrieves user and user-group role grants for the specified datasets from Keycloak context-grant groups.
		/// </summary>
		/// <param name="datasetIds">
		/// The dataset IDs to look up. Duplicate IDs are processed once.
		/// If null or empty, an empty list is returned without contacting Keycloak.
		/// </param>
		/// <returns>
		/// A list containing one entry per distinct dataset ID. Each entry contains separate collections of users and user groups, together with their dataset roles.
		/// Datasets without matching grants are included with empty principal collections.
		/// </returns>
		/// <remarks>
		/// Obtains a client access token and searches Keycloak separately for each dataset ID as an exact group-name search term. Results are retrieved in pages of {QueryingPaginationSize} matching groups.
		/// 
		/// Processes principal groups directly beneath the configured context-grant root, identified by its path. Each principal group's configured type attribute determines whether it represents a user or a user group.
		/// Groups with missing or unrecognized type attributes are skipped. If both types are present, the principal is treated as a user.
		/// 
		/// For user principals, the principal group's name is used as the principal ID. For user-group principals, the Keycloak group ID is used instead.
		/// Direct children of each principal group are inspected for names that parse as the requested dataset ID. Matching target groups with a nonempty group ID are processed only once per dataset across all pages.
		/// 
		/// Converts each matching target group into context grants and retains only roles whose target type is a dataset. Target groups that produce no grants or no dataset roles are skipped. Roles are merged for principals with the same ID using an ordinal, case-insensitive ID comparison.
		/// 
		/// After each page, recursively counts groups whose names match the searched dataset ID throughout the returned hierarchy. Ancestor groups with different names do not contribute to this count. A count below the page size ends pagination; otherwise, the offset advances by {QueryingPaginationSize} and another page is requested.
		/// A full final page therefore results in an additional request.
		/// 
		/// The pagination stopping condition assumes that the returned hierarchy contains all matching groups selected for the requested page.
		/// </remarks>
		public async Task<List<DatasetContextGrants>> LookupDatasetContextGrants(IEnumerable<Guid> datasetIds)
		{
			if (datasetIds == null) return [];
			HashSet<Guid> datasetIdsMap = datasetIds.ToHashSet();
			if (datasetIdsMap.Count == 0) return [];

			string token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Dictionary<Guid, DatasetContextGrants> result = datasetIdsMap.ToDictionary(x => x, x => new DatasetContextGrants
			{
				DatasetId = x,
				Users = [],
				UserGroups = []
			});

			foreach (Guid datasetId in datasetIdsMap)
			{
				string datasetIdToUse = datasetId.ToString().ToLowerInvariant();
				int first = 0;
				HashSet<string> processedTargetGroupIds = [];
				while (true)
				{
					using HttpRequestMessage request = new HttpRequestMessage(
						HttpMethod.Get,
						$"{this._config.BaseUrl}{this._config.GroupsEndpoint}?briefRepresentation=false&search={datasetIdToUse}&exact=true&first={first}&max={this._config.QueryingPaginationSize}");
					request.Headers.Add(HeaderNames.Accept, "application/json");
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

					string content = await this.SendRequest(request);
					List<Model.Group> roots;
					try
					{
						roots = this._jsonHandlingService.FromJson<List<Model.Group>>(content) ?? [];
					}
					catch (System.Exception ex)
					{
						this._logger.LogError(ex, "Failed to parse response: {content}", content);
						throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
					}

					int pageResultCount = CountGroupsByName(roots, datasetIdToUse);
					foreach (Model.Group principalGroup in roots
						.Where(x => x.Path != null && x.Path.Equals($"/{this._config.ContextGrantGroupPrefix}", StringComparison.OrdinalIgnoreCase))
						.SelectMany(x => x.SubGroups ?? []))
					{
						if (principalGroup.Attributes == null || !principalGroup.Attributes.TryGetValue(this._config.ContextGrantTypeAttributeName, out List<string> principalTypes) || principalTypes == null) continue;

						bool isUser = principalTypes.Any(x => string.Equals(x, this._config.ContextGrantTypeUserAttributeValue, StringComparison.OrdinalIgnoreCase));
						bool isGroup = principalTypes.Any(x => string.Equals(x, this._config.ContextGrantTypeGroupAttributeValue, StringComparison.OrdinalIgnoreCase));
						if (!isUser && !isGroup) continue;

						string principalId = isUser ? principalGroup.Name : principalGroup.Id;
						foreach (Model.Group targetGroup in principalGroup.SubGroups ?? [])
						{
							if (!Guid.TryParse(targetGroup.Name, out Guid targetDatasetId) || targetDatasetId != datasetId) continue;
							if (!String.IsNullOrEmpty(targetGroup.Id) && !processedTargetGroupIds.Add(targetGroup.Id)) continue;
							List<ContextGrant> grants = this.ConvertToContextGrant(principalId, principalGroup, targetGroup);
							if (grants == null) continue;
							HashSet<string> roles = grants.Where(x => x.TargetType == ContextGrant.TargetKind.Dataset).Select(x => x.Role).ToHashSet();
							if (roles.Count == 0) continue;
							List<DatasetRoles> principals = isUser ? result[datasetId].Users : result[datasetId].UserGroups;
							DatasetRoles existing = principals.FirstOrDefault(x => string.Equals(x.Id, principalId, StringComparison.OrdinalIgnoreCase));

							if (existing == null)
							{
								principals.Add(new DatasetRoles
								{
									Id = principalId,
									Roles = roles
								});
							}
							else
							{
								existing.Roles.UnionWith(roles);
							}
						}
					}

					if (pageResultCount < this._config.QueryingPaginationSize) break;
					first += this._config.QueryingPaginationSize;
				}
			}

			return result.Values.ToList();
		}

		/// <summary>
		/// Recursively counts groups whose names match the specified name, including groups at every level of the supplied hierarchy.
		/// </summary>
		/// <param name="groups">
		/// The groups to inspect. If null, the count is zero.
		/// </param>
		/// <param name="name">
		/// The group name to match using an ordinal, case-insensitive comparison.
		/// </param>
		/// <returns>
		/// The total number of matching groups in the supplied collection and all nested subgroups.
		/// </returns>
		private static int CountGroupsByName(IEnumerable<Model.Group> groups, string name)
		{
			if (groups == null) return 0;
			int count = 0;
			foreach (Model.Group group in groups)
			{
				if (string.Equals(group.Name, name, StringComparison.OrdinalIgnoreCase)) count++;
				count += CountGroupsByName(group.SubGroups, name);
			}
			return count;
		}
	}
}
