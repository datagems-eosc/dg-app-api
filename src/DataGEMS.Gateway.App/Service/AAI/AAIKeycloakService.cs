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

			HttpRequestMessage lookupHttpRequest = null;
			if (String.IsNullOrEmpty(parentId)) lookupHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupsEndpoint}");
			else lookupHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupChildrenEndpoint.Replace("{groupId}", parentId)}");
			lookupHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String groupsContent = await this.SendRequest(lookupHttpRequest);
			List<Model.Group> groups = null;
			try { groups = String.IsNullOrEmpty(groupsContent) ? new List<Model.Group>() : this._jsonHandlingService.FromJson<List<Model.Group>>(groupsContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}

			if (groups != null && groups.Count(x => x.Name == name) == 1) return groups.FirstOrDefault(x => x.Name == name)?.Id;
			else if (groups != null && groups.Count(x => x.Name == name) > 1) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			else
			{
				Model.Group create = new Model.Group() { Name = name, Attributes = attributes };

				String requestUrl = null;
				if (String.IsNullOrEmpty(parentId)) requestUrl = $"{this._config.BaseUrl}{this._config.GroupsEndpoint}";
				else requestUrl = $"{this._config.BaseUrl}{this._config.GroupChildrenEndpoint.Replace("{groupId}", parentId)}";

				HttpRequestMessage createHttpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
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

		private List<ContextGrant> ConvertToContextGrant(String principalId, Model.Group principalGroup, Model.Group targetGroup)
		{
			if (!Guid.TryParse(targetGroup.Name, out Guid targetId)) return null;

			if (targetGroup.Attributes == null ||
				!targetGroup.Attributes.ContainsKey(this._config.ContextGrantTypeAttributeName) ||
				targetGroup.Attributes[this._config.ContextGrantTypeAttributeName] == null ||
				targetGroup.Attributes[this._config.ContextGrantTypeAttributeName].Count == 0) return null;

			ContextGrant.PrincipalKind principalType;
			if (principalGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeUserAttributeValue, StringComparison.OrdinalIgnoreCase))) principalType = ContextGrant.PrincipalKind.User;
			else if (principalGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeGroupAttributeValue, StringComparison.OrdinalIgnoreCase))) principalType = ContextGrant.PrincipalKind.Group;
			else return null;

			ContextGrant.TargetKind targetType;
			if (targetGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeDatasetAttributeValue, StringComparison.OrdinalIgnoreCase))) targetType = ContextGrant.TargetKind.Dataset;
			else if (targetGroup.Attributes[this._config.ContextGrantTypeAttributeName].Any(x => x.Equals(this._config.ContextGrantTypeCollectionAttributeValue, StringComparison.OrdinalIgnoreCase))) targetType = ContextGrant.TargetKind.Collection;
			else return null;

			if (targetGroup.RealmRoles == null || targetGroup.RealmRoles.Count == 0) return null;

			List<string> semantics = null;
			if (principalGroup.Attributes.ContainsKey(this._config.ContextSemanticsAttributeName) && principalGroup.Attributes[this._config.ContextSemanticsAttributeName] != null && principalGroup.Attributes[this._config.ContextSemanticsAttributeName].Count > 0)
			{
				semantics = principalGroup.Attributes[this._config.ContextSemanticsAttributeName];
			}

			List<ContextGrant> targetGrants = targetGroup.RealmRoles.Select(x => new ContextGrant()
			{
				PrincipalId = principalId,
				PrincipalType = principalType,
				TargetType = targetType,
				TargetId = targetId,
				Role = x,
				Semantics = semantics,
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
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
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

			if (locationHeaderReturn) content = response.Headers.Location?.ToString();
			return content;
		}
	}
}
