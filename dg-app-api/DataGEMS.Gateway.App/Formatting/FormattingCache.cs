using Cite.Tools.Cache;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Event;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Formatting
{
	public class FormattingCache
	{
		private readonly ILogger<FormattingCache> _logger;
		private readonly IDistributedCache _cache;
		private readonly JsonHandlingService _jsonService;
		private readonly FormattingCacheConfig _config;
		private readonly EventBroker _eventBroker;
		private readonly IServiceProvider _serviceProvider;

		public FormattingCache(
			ILogger<FormattingCache> logger,
			IDistributedCache cache,
			JsonHandlingService jsonHandlingService,
			FormattingCacheConfig config,
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
			this._eventBroker.UserTouched += OnUserTouched;
			this._eventBroker.UserDeleted += OnUserDeleted;
			this._eventBroker.UserProfileDeleted += OnUserProfileDeleted;
		}

		private async void OnUserProfileDeleted(object sender, OnEventArgs e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserProfileDeleted))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("userIds", e.Ids));
			await this.PurgeCache(e.Ids);
		}

		private async void OnUserProfileTouched(object sender, OnEventArgs e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserProfileDeleted))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("userIds", e.Ids));
			await this.PurgeCache(e.Ids);
		}

		private async void OnUserDeleted(object sender, OnEventArgs e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserDeleted))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("userIds", e.Ids));
			await this.PurgeCache(e.Ids);
		}

		private async void OnUserTouched(object sender, OnEventArgs e)
		{
			this._logger.Debug(new MapLogEntry("received event")
				.And("event", nameof(OnUserTouched))
				.And("prefix", this._config.LookupCache?.Prefix)
				.And("pattern", this._config.LookupCache?.KeyPattern)
				.And("userIds", e.Ids));
			await this.PurgeCache(e.Ids);
		}

		public async Task<UserFormattingProfile> CacheLookup(Guid userId)
		{
			String cacheKey = this.CacheKey(userId);
			String content = await this._cache.GetStringAsync(cacheKey);
			if (String.IsNullOrEmpty(content)) return null;

			return _jsonService.FromJsonSafe<UserFormattingProfile>(content);
		}

		public async Task CacheUpdate(Guid userId, UserFormattingProfile content)
		{
			String cacheKey = this.CacheKey(userId);
			await this._cache.SetStringAsync(cacheKey, _jsonService.ToJsonSafe(content), this._config.LookupCache.ToOptions());
		}

		private async Task PurgeCache(IEnumerable<Guid> userIds)
		{
			if (userIds == null) return;
			try
			{
				foreach (Guid userId in userIds)
				{
					String cacheKey = this.CacheKey(userId);
					await this._cache.RemoveAsync(cacheKey);
				}
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, new MapLogEntry("problem invalidating cache entry. skipping")
					.And("prefix", this._config.LookupCache?.Prefix)
					.And("pattern", this._config.LookupCache?.KeyPattern)
					.And("users", userIds));
			}
		}

		private String CacheKey(Guid userId)
		{
			String cacheKey = this._config.LookupCache.ToKey(new KeyValuePair<String, String>[] {
				new KeyValuePair<string, string>("{prefix}", this._config.LookupCache.Prefix),
				new KeyValuePair<string, string>("{key}", userId.ToString())
			});
			return cacheKey;
		}
	}
}
