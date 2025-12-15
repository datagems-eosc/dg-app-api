using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Query
{
	public class UserGroupHttpQuery : Cite.Tools.Data.Query.IQuery
	{
		private List<String> _ids { get; set; }
		private List<String> _excludedIds { get; set; }
		private List<string> _semantics { get; set; }
		private String _like { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.AAI.AAIConfig _config;
		private readonly ILogger<UserGroupHttpQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly IAccessTokenService _accessTokenService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public UserGroupHttpQuery(
			IHttpClientFactory httpClientFactory,
			Service.AAI.AAIConfig config,
			ILogger<UserGroupHttpQuery> logger,
			JsonHandlingService jsonHandlingService,
			LogCorrelationScope logCorrelationScope,
			IAccessTokenService accessTokenService,
			IAuthorizationContentResolver authorizationContentResolver,
			ErrorThesaurus errors)
		{
			this._httpClientFactory = httpClientFactory;
			this._config = config;
			this._logger = logger;
			this._accessTokenService = accessTokenService;
			this._jsonHandlingService = jsonHandlingService;
			this._logCorrelationScope = logCorrelationScope;
			this._errors = errors;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public UserGroupHttpQuery Ids(IEnumerable<String> ids) { this._ids = ids?.ToList(); return this; }
		public UserGroupHttpQuery Ids(String id) { this._ids = id.AsList(); return this; }
		public UserGroupHttpQuery ExcludedIds(IEnumerable<String> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public UserGroupHttpQuery ExcludedIds(String excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public UserGroupHttpQuery Semantics(IEnumerable<string> semantics) { this._semantics = semantics?.ToList(); return this; }
		public UserGroupHttpQuery Semantics(string semantics) { this._semantics = semantics.AsList(); return this; }
		public UserGroupHttpQuery Like(String like) { this._like = like; return this; }
		public UserGroupHttpQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected bool IsFalseQuery()
		{
			return this._ids.IsNotNullButEmpty() || this._excludedIds.IsNotNullButEmpty() || this._semantics.IsNotNullButEmpty();
		}

		public async Task<List<Service.AAI.Model.Group>> CollectAsync()
		{
			if (this.IsFalseQuery()) return new List<Service.AAI.Model.Group>();

			List<Service.AAI.Model.Group> items = await this.CollectBaseAsync();
			if (this._ids != null) items = items.Where(x => this._ids.Contains(x.Id)).ToList();
			if (this._excludedIds != null) items = items.Where(x => !this._excludedIds.Contains(x.Id)).ToList();
			if (this._semantics != null) items = items.Where(x => 
				x.Attributes != null 
				&& x.Attributes.ContainsKey(this._config.ContextSemanticsAttributeName)
				&& this._semantics.Any(y => x.Attributes[this._config.ContextSemanticsAttributeName].Contains(y))
			).ToList();
			if (!String.IsNullOrEmpty(this._like)) items = items.Where(x => x.Name.Contains(this._like)).ToList();
			return items;
		}

		private async Task<List<Service.AAI.Model.Group>> CollectBaseAsync()
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None))
			{
				return await this.CollectAllGroups();
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseUserGroup)) return await this.CollectAllGroups();
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = await this._authorizationContentResolver.SubjectIdOfCurrentUser();
				if (!String.IsNullOrEmpty(currentUser))
				{
					HashSet<String> userGroups = await this.CollectMemberGroups(currentUser);
					List<Service.AAI.Model.Group> groups = await this.CollectAllGroups();
					List<Service.AAI.Model.Group> memberGroups = groups.Where(x => userGroups.Contains(x.Id)).ToList();
					return memberGroups;
				}
			}
			//AuthorizationFlags.Context not applicable
			return new List<Service.AAI.Model.Group>();
		}

		private async Task<List<Service.AAI.Model.Group>> CollectAllGroups()
		{
			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage lookupRootHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupsEndpoint}?search={this._config.ContextGrantGroupPrefix}");
			lookupRootHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
			lookupRootHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			String rootContent = await this.SendRequest(lookupRootHttpRequest);
			List<Service.AAI.Model.Group> roots = null;
			try { roots = String.IsNullOrEmpty(rootContent) ? new List<Service.AAI.Model.Group>() : this._jsonHandlingService.FromJson<List<Service.AAI.Model.Group>>(rootContent); }
			catch (System.Exception ex)
			{
				this._logger.LogError(ex, "Failed to parse response: {content}", rootContent);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
			}
			String rootId = roots.Where(x => x.Name.Equals(this._config.ContextGrantGroupPrefix, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Id;
			if (String.IsNullOrEmpty(rootId)) return new List<Service.AAI.Model.Group>();

			List<Service.AAI.Model.Group> groups = new List<Service.AAI.Model.Group>();
			int first = 0;
			int max = 100;

			while (true)
			{
				HttpRequestMessage lookupChildrenHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.GroupChildrenEndpoint.Replace("{groupId}", rootId)}?briefRepresentation=false&first={first}&max={max}");
				lookupChildrenHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
				lookupChildrenHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				String groupsContent = await this.SendRequest(lookupChildrenHttpRequest);
				List<Service.AAI.Model.Group> pageGroups = null;
				try { pageGroups = String.IsNullOrEmpty(groupsContent) ? new List<Service.AAI.Model.Group>() : this._jsonHandlingService.FromJson<List<Service.AAI.Model.Group>>(groupsContent); }
				catch (System.Exception ex)
				{
					this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
					throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
				}
				List<Service.AAI.Model.Group> filteredGroups = pageGroups.Where(x =>
					x.Attributes != null &&
					x.Attributes.ContainsKey(this._config.ContextGrantTypeAttributeName) &&
					x.Attributes[this._config.ContextGrantTypeAttributeName].Contains(this._config.ContextGrantTypeGroupAttributeValue)).ToList();
				groups.AddRange(filteredGroups);
				if (pageGroups.Count < max) break;
				first += max;
			}

			return groups;
		}

		private async Task<HashSet<String>> CollectMemberGroups(String subjectId)
		{
			String token = await this._accessTokenService.GetClientAccessTokenAsync(this._config.Scope);
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			List<String> groups = new List<String>();
			int first = 0;
			int max = 100;

			while (true)
			{
				HttpRequestMessage lookupSubjectGroupsHttpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.UserGroupsEndpoint.Replace("{userId}", subjectId.ToLowerInvariant())}?briefRepresentation=false&first={first}&max={max}");
				lookupSubjectGroupsHttpRequest.Headers.Add(HeaderNames.Accept, "application/json");
				lookupSubjectGroupsHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				String groupsContent = await this.SendRequest(lookupSubjectGroupsHttpRequest);
				List<Service.AAI.Model.Group> pageGroups = null;
				try { pageGroups = String.IsNullOrEmpty(groupsContent) ? new List<Service.AAI.Model.Group>() : this._jsonHandlingService.FromJson<List<Service.AAI.Model.Group>>(groupsContent); }
				catch (System.Exception ex)
				{
					this._logger.LogError(ex, "Failed to parse response: {content}", groupsContent);
					throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI, this._logCorrelationScope.CorrelationId);
				}
				List<String> filteredGroupIds = pageGroups.Where(x =>
					x.Path.StartsWith($"/{this._config.ContextGrantGroupPrefix}") &&
					x.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length == 2 &&
					x.Attributes != null &&
					x.Attributes.ContainsKey(this._config.ContextGrantTypeAttributeName) &&
					x.Attributes[this._config.ContextGrantTypeAttributeName].Contains(this._config.ContextGrantTypeGroupAttributeValue))
					.Select(x=> x.Id).ToList();
				groups.AddRange(filteredGroupIds);
				if (pageGroups.Count < max) break;
				first += max;
			}
			return groups.ToHashSet();
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
