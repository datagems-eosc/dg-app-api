using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Event;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Cite.Tools.Cache;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public class AAICache
	{
		private readonly ILogger<AAICache> _logger;
		private readonly IDistributedCache _cache;
		private readonly JsonHandlingService _jsonService;
		private readonly AAICacheConfig _config;
		private readonly EventBroker _eventBroker;
		private readonly IServiceProvider _serviceProvider;

		public AAICache(
			ILogger<AAICache> logger,
			IDistributedCache cache,
			JsonHandlingService jsonHandlingService,
			AAICacheConfig config,
			EventBroker eventBroker,
			IServiceProvider serviceProvider)
		{
			this._logger = logger;
			this._config = config;
			this._cache = cache;
			this._jsonService = jsonHandlingService;
			this._eventBroker = eventBroker;
			this._serviceProvider = serviceProvider;
		}

		//TODO: Work on cache invalidation
		public void RegisterListener()
		{
			this._eventBroker.UserTouched += OnUserTouched;
			this._eventBroker.UserDeleted += OnUserDeleted;
		}

		private async void OnUserDeleted(object sender, OnUserEventArgs e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserDeleted))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("groupIds", e.Ids));
			await this.PurgePrincipalGroupCache(e.Ids?.Select(x => x.SubjectId).ToList());
		}

		private async void OnUserTouched(object sender, OnUserEventArgs e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserTouched))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("groupIds", e.Ids));
			await this.PurgePrincipalGroupCache(e.Ids?.Select(x => x.SubjectId).ToList());
		}

		#region Principal Group

		private String CachePrincipalGroupKey(String principalId)
		{
			String cacheKey = this._config.LookupCache.ToKey(new KeyValuePair<String, String>[] {
				new KeyValuePair<string, string>("{prefix}", this._config.LookupCache.Prefix),
				new KeyValuePair<string, string>("{kind}", "principal-group"),
				new KeyValuePair<string, string>("{key}", principalId)
			});
			return cacheKey;
		}

		public async Task<Model.Group> PrincipalGroupLookup(String principalId)
		{
			String cacheKey = this.CachePrincipalGroupKey(principalId);
			String content = await this._cache.GetStringAsync(cacheKey);
			if (String.IsNullOrEmpty(content)) return null;

			return _jsonService.FromJsonSafe<Model.Group>(content);
		}

		public async Task PrincipalGroupUpdate(String principalId, Model.Group content)
		{
			String cacheKey = this.CachePrincipalGroupKey(principalId);
			await this._cache.SetStringAsync(cacheKey, _jsonService.ToJsonSafe(content), this._config.LookupCache.ToOptions());
		}

		private async Task PurgePrincipalGroupCache(IEnumerable<String> principalIds)
		{
			if (principalIds == null) return;
			try
			{
				foreach (String principalId in principalIds)
				{
					String cacheKey = this.CachePrincipalGroupKey(principalId);
					await this._cache.RemoveAsync(cacheKey);
				}
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, new MapLogEntry("problem invalidating cache entry. skipping")
					.And("prefix", this._config.LookupCache?.Prefix)
					.And("pattern", this._config.LookupCache?.KeyPattern)
					.And("principals", principalIds));
			}
		}

		#endregion

		#region Subgroups

		private String CacheSubGroupsKey(String parentId)
		{
			String cacheKey = this._config.LookupCache.ToKey(new KeyValuePair<String, String>[] {
				new KeyValuePair<string, string>("{prefix}", this._config.LookupCache.Prefix),
				new KeyValuePair<string, string>("{kind}", "sub-groups"),
				new KeyValuePair<string, string>("{key}", parentId)
			});
			return cacheKey;
		}

		public async Task<List<Model.Group>> SubGroupsLookup(String parentId)
		{
			String cacheKey = this.CacheSubGroupsKey(parentId);
			String content = await this._cache.GetStringAsync(cacheKey);
			if (String.IsNullOrEmpty(content)) return null;

			return _jsonService.FromJsonSafe<List<Model.Group>>(content);
		}

		public async Task SubGroupsUpdate(String parentId, List<Model.Group> content)
		{
			String cacheKey = this.CacheSubGroupsKey(parentId);
			await this._cache.SetStringAsync(cacheKey, _jsonService.ToJsonSafe(content), this._config.LookupCache.ToOptions());
		}

		private async Task PurgeSubGroupsCache(IEnumerable<String> parentIds)
		{
			if (parentIds == null) return;
			try
			{
				foreach (String parentId in parentIds)
				{
					String cacheKey = this.CacheSubGroupsKey(parentId);
					await this._cache.RemoveAsync(cacheKey);
				}
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, new MapLogEntry("problem invalidating cache entry. skipping")
					.And("prefix", this._config.LookupCache?.Prefix)
					.And("pattern", this._config.LookupCache?.KeyPattern)
					.And("parents", parentIds));
			}
		}

		#endregion

		#region User Groups

		private String CacheUserGroupsKey(String userSubjectId)
		{
			String cacheKey = this._config.LookupCache.ToKey(new KeyValuePair<String, String>[] {
				new KeyValuePair<string, string>("{prefix}", this._config.LookupCache.Prefix),
				new KeyValuePair<string, string>("{kind}", "user-groups"),
				new KeyValuePair<string, string>("{key}", userSubjectId)
			});
			return cacheKey;
		}

		public async Task<List<Model.Group>> UserGroupsLookup(String userSubjectId)
		{
			String cacheKey = this.CacheUserGroupsKey(userSubjectId);
			String content = await this._cache.GetStringAsync(cacheKey);
			if (String.IsNullOrEmpty(content)) return null;

			return _jsonService.FromJsonSafe<List<Model.Group>>(content);
		}

		public async Task UserGroupsUpdate(String userSubjectId, List<Model.Group> content)
		{
			String cacheKey = this.CacheUserGroupsKey(userSubjectId);
			await this._cache.SetStringAsync(cacheKey, _jsonService.ToJsonSafe(content), this._config.LookupCache.ToOptions());
		}

		private async Task PurgeUserGroupsCache(IEnumerable<String> userSubjectIds)
		{
			if (userSubjectIds == null) return;
			try
			{
				foreach (String userSubjectId in userSubjectIds)
				{
					String cacheKey = this.CacheUserGroupsKey(userSubjectId);
					await this._cache.RemoveAsync(cacheKey);
				}
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, new MapLogEntry("problem invalidating cache entry. skipping")
					.And("prefix", this._config.LookupCache?.Prefix)
					.And("pattern", this._config.LookupCache?.KeyPattern)
					.And("userSubjects", userSubjectIds));
			}
		}

		#endregion
	}
}
