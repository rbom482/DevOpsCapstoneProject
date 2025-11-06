using Microsoft.Extensions.Caching.Memory;
using LogiTrack.Models;

namespace LogiTrack.Services
{
    public interface ICacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
    }

    public class EnhancedCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<EnhancedCacheService> _logger;
        private readonly Dictionary<string, DateTime> _cacheIndex;

        public EnhancedCacheService(IMemoryCache memoryCache, ILogger<EnhancedCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _cacheIndex = new Dictionary<string, DateTime>();
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug("Cache HIT for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache MISS for key: {Key}, fetching from source", key);
            var value = await factory();
            
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _memoryCache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes / 2, 30)),
                Priority = CacheItemPriority.Normal,
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            _logger.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", key, reason);
                            _cacheIndex.Remove(key.ToString() ?? string.Empty);
                        }
                    }
                }
            };

            _memoryCache.Set(key, value, options);
            _cacheIndex[key] = DateTime.UtcNow.Add(expiration);
            
            _logger.LogDebug("Cache SET for key: {Key}, expires at: {Expiration}", key, _cacheIndex[key]);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _cacheIndex.Remove(key);
            _logger.LogDebug("Cache REMOVE for key: {Key}", key);
            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            var keysToRemove = _cacheIndex.Keys
                .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheIndex.Remove(key);
            }

            _logger.LogDebug("Cache REMOVE by pattern: {Pattern}, removed {Count} entries", pattern, keysToRemove.Count);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var exists = _cacheIndex.ContainsKey(key) && _cacheIndex[key] > DateTime.UtcNow;
            return Task.FromResult(exists);
        }
    }
}