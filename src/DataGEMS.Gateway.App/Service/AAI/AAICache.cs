using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Event;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Cite.Tools.Cache;
using DataGEMS.Gateway.App.Common.Auth;

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

		public void RegisterListener()
		{
			this._eventBroker.CollectionTouched += OnCollectionTouched;
			this._eventBroker.CollectionDeleted += OnCollectionDeleted;
			this._eventBroker.UserDatasetGrantTouched += OnUserDatasetGrantTouched;
			this._eventBroker.UserDatasetGrantDeleted += OnUserDatasetGrantDeleted;
		}

		private async void OnCollectionDeleted(object sender, OnEventArgs<Guid> e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnCollectionDeleted))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("groupIds", e.Ids));
			await this.PurgeGroupCache(e.Ids?.Select(x => x.ToString()).ToList());
		}

		private async void OnCollectionTouched(object sender, OnEventArgs<Guid> e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnCollectionTouched))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("groupIds", e.Ids));
			await this.PurgeGroupCache(e.Ids?.Select(x => x.ToString()).ToList());
		}

		private async void OnUserDatasetGrantDeleted(object sender, OnEventArgs<String> e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserDatasetGrantDeleted))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("userIds", e.Ids));
			await this.PurgeUserDatasetGrantCache(e.Ids);
		}

		private async void OnUserDatasetGrantTouched(object sender, OnEventArgs<String> e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserDatasetGrantTouched))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("userIds", e.Ids));
			await this.PurgeUserDatasetGrantCache(e.Ids);
		}

		public async Task<List<DatasetGrant>> CacheGroupLookup(String groupId)
		{
			String cacheKey = this.CacheGroupKey(groupId);
			String content = await this._cache.GetStringAsync(cacheKey);
			if (String.IsNullOrEmpty(content)) return null;

			return _jsonService.FromJsonSafe<List<DatasetGrant>>(content);
		}

		public async Task<List<DatasetGrant>> CacheUserDatasetGrantLookup(String subjectId)
		{
			String cacheKey = this.CacheUserDatasetGrantKey(subjectId);
			String content = await this._cache.GetStringAsync(cacheKey);
			if (String.IsNullOrEmpty(content)) return null;

			return _jsonService.FromJsonSafe<List<DatasetGrant>>(content);
		}

		public async Task CacheGroupUpdate(String groupId, List<DatasetGrant> content)
		{
			String cacheKey = this.CacheGroupKey(groupId);
			await this._cache.SetStringAsync(cacheKey, _jsonService.ToJsonSafe(content), this._config.LookupCache.ToOptions());
		}

		public async Task CacheUserDatasetGrantUpdate(String subjectId, List<DatasetGrant> content)
		{
			String cacheKey = this.CacheUserDatasetGrantKey(subjectId);
			await this._cache.SetStringAsync(cacheKey, _jsonService.ToJsonSafe(content), this._config.LookupCache.ToOptions());
		}

		private async Task PurgeGroupCache(IEnumerable<String> groupIds)
		{
			if (groupIds == null) return;
			try
			{
				foreach (String groupId in groupIds)
				{
					String cacheKey = this.CacheGroupKey(groupId);
					await this._cache.RemoveAsync(cacheKey);
				}
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, new MapLogEntry("problem invalidating cache entry. skipping")
					.And("prefix", this._config.LookupCache?.Prefix)
					.And("pattern", this._config.LookupCache?.KeyPattern)
					.And("groups", groupIds));
			}
		}

		private async Task PurgeUserDatasetGrantCache(IEnumerable<String> subjectIds)
		{
			if (subjectIds == null) return;
			try
			{
				foreach (String subjectId in subjectIds)
				{
					String cacheKey = this.CacheUserDatasetGrantKey(subjectId);
					await this._cache.RemoveAsync(cacheKey);
				}
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, new MapLogEntry("problem invalidating cache entry. skipping")
					.And("prefix", this._config.LookupCache?.Prefix)
					.And("pattern", this._config.LookupCache?.KeyPattern)
					.And("users", subjectIds));
			}
		}

		private String CacheGroupKey(String groupId)
		{
			String cacheKey = this._config.LookupCache.ToKey(new KeyValuePair<String, String>[] {
				new KeyValuePair<string, string>("{prefix}", this._config.LookupCache.Prefix),
				new KeyValuePair<string, string>("{kind}", "group-dataset-grant"),
				new KeyValuePair<string, string>("{key}", groupId.ToString())
			});
			return cacheKey;
		}

		private String CacheUserDatasetGrantKey(String subjectId)
		{
			String cacheKey = this._config.LookupCache.ToKey(new KeyValuePair<String, String>[] {
				new KeyValuePair<string, string>("{prefix}", this._config.LookupCache.Prefix),
				new KeyValuePair<string, string>("{kind}", "user-dataset-grant"),
				new KeyValuePair<string, string>("{key}", subjectId)
			});
			return cacheKey;
		}
	}
}
