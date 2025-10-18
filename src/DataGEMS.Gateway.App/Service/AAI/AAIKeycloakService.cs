using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Auth;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Service.AAI.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

		public async Task<List<DatasetGrant>> LookupDatasetGrantGroups(String code)
		{
			if (String.IsNullOrEmpty(code)) throw new DGApplicationException(this._errors.ModelValidation.Code, this._errors.ModelValidation.Message);

			List<DatasetGrant> cachedGrants = await this._aaiCache.CacheGroupLookup(code);
			if (cachedGrants != null) return cachedGrants;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage lookupParentHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupsEndpoint}?search={code}");
			lookupParentHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupParentHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String groupsContent = await this.SendRequest(lookupParentHttpRequest);
			List<Model.Group> groups = null;
			try { groups = this._jsonHandlingService.FromJson<List<Model.Group>>(groupsContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			if(groups == null || groups.Count !=1 || String.IsNullOrEmpty(groups[0].Id)) throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			String codeGroupId = groups[0].Id;


			HttpRequestMessage lookupGrantsHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupChildrenEndpoint.Replace("{groupId}", codeGroupId)}");
			lookupGrantsHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupGrantsHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String grantsContent = await this.SendRequest(lookupGrantsHttpRequest);
			List<Model.Group> grantGroups = null;
			try { grantGroups = this._jsonHandlingService.FromJson<List<Model.Group>>(grantsContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", grantsContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}

			List<DatasetGrant> grants = this.ParseDatasetGrants(grantGroups.Select(x => new DatasetGrantInfo() { Id = x.Id, Path = x.Path }).ToList());
			await this._aaiCache.CacheGroupUpdate(code, grants);

			return grants;
		}

		public async Task BootstrapDatasetGrant(DatasetGrant.TargetType type, String id)
		{
			if (String.IsNullOrEmpty(id)) throw new DGApplicationException(this._errors.ModelValidation.Code, this._errors.ModelValidation.Message);

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			List<String> grantNames = null;
			String targetName = null;
			switch (type)
			{
				case DatasetGrant.TargetType.Dataset:
					{
						targetName = this._config.HierarchyDatasetDirectLevelName;
						grantNames = this._config.SubDirectGrantNames;
						break;
					}
				case DatasetGrant.TargetType.Group:
					{
						targetName = this._config.HierarchyDatasetGroupLevelName;
						grantNames = this._config.SubGroupGrantNames;
						break;
					}
				default: throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			String topLevelId = await this.EnsureHierarchyTargetLevel(null, this._config.HierarchyDatasetTopLevelName);
			String targetLevelId = await this.EnsureHierarchyTargetLevel(topLevelId, targetName);
			String currentLevelId = await this.EnsureHierarchyTargetLevel(targetLevelId, targetName);


			foreach (String subGroup in grantNames)
			{
				await this.EnsureHierarchyTargetLevel(currentLevelId, subGroup);
			}

		}

		private async Task<String> EnsureHierarchyTargetLevel(String parentId, String target)
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
			try { groups = this._jsonHandlingService.FromJson<List<Model.Group>>(groupsContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}

			String targetId = null;
			if (groups != null && groups.Count(x => x.Name == target) == 1)
			{
				targetId = groups.FirstOrDefault(x => x.Name == target)?.Id;
			}
			else if (groups != null && groups.Count(x => x.Name == target) > 1)
			{
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			else
			{
				Model.Group create = new Model.Group() { Name = target };

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
				targetId = locationParts.Last();
			}
			return targetId;
		}

		public async Task AddUserToGroup(String subjectId, String groupId)
		{
			if (String.IsNullOrEmpty(subjectId) || String.IsNullOrEmpty(groupId)) return;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{this._config.BaseUrl}{this._config.UserGroupsChangeEndpoint.Replace("{userId}", subjectId).Replace("{groupId}", groupId)}");
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			await this.SendRequest(httpRequest);

			this._eventBroker.EmitUserDatasetGrantTouched(subjectId);
		}

		public async Task RemoveUserFromGroup(String subjectId, String groupId)
		{
			if (String.IsNullOrEmpty(subjectId) || String.IsNullOrEmpty(groupId)) return;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{this._config.BaseUrl}{this._config.UserGroupsChangeEndpoint.Replace("{userId}", subjectId).Replace("{groupId}", groupId)}");
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			await this.SendRequest(httpRequest);

			this._eventBroker.EmitUserDatasetGrantDeleted(subjectId);
		}

		public async Task<List<DatasetGrant>> UserDatasetGrants(String subjectId)
		{
			if(String.IsNullOrEmpty(subjectId)) return Enumerable.Empty<DatasetGrant>().ToList();

			List<DatasetGrant> cachedGrants = await this._aaiCache.CacheUserDatasetGrantLookup(subjectId);
			if(cachedGrants != null) return cachedGrants;

			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.UserGroupsEndpoint.Replace("{userId}", subjectId)}");
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String content = await this.SendRequest(httpRequest);
			List<UserGroupMembership> response = null;
			try { response = this._jsonHandlingService.FromJson<List<UserGroupMembership>>(content); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}

			List<DatasetGrant> grants = this.ParseDatasetGrants(response.Select(x => new DatasetGrantInfo() { Id = x.Id, Path = x.Path }).ToList());
			await this._aaiCache.CacheUserDatasetGrantUpdate(subjectId, grants);

			return grants;
		}

		private class DatasetGrantInfo
		{
			public String Id { get; set; }
			public String Path { get; set; }
		}

		private List<DatasetGrant> ParseDatasetGrants(List<DatasetGrantInfo> items)
		{
			List<DatasetGrant> grants = new List<DatasetGrant>();
			foreach (DatasetGrantInfo item in items)
			{
				String[] membershipPath = item.Path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if (membershipPath.Length != 4 ||
					membershipPath.Any(x => String.IsNullOrEmpty(x)) ||
					!String.Equals(membershipPath[0], "dataset", StringComparison.OrdinalIgnoreCase)) continue;

				DatasetGrant.TargetType type;
				switch (membershipPath[1])
				{
					case "group": { type = DatasetGrant.TargetType.Group; break; }
					case "direct": { type = DatasetGrant.TargetType.Dataset; break; }
					default: continue;
				}

				grants.Add(new DatasetGrant
				{
					GroupId = item.Id,
					Type = type,
					Code = membershipPath[2],
					Access = membershipPath[3],
				});
			}
			return grants;
		}

		private async Task<string> SendRequest(HttpRequestMessage request, Boolean locationHeaderReturn = false)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.InDataExploration, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.InDataExploration, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			String content = await response.Content.ReadAsStringAsync();

			if (locationHeaderReturn) content = response.Headers.Location?.ToString();
			return content;
		}
	}
}
