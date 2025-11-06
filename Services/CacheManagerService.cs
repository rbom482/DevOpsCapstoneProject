using LogiTrack.Services;

namespace LogiTrack.Services
{
    public interface ICacheManagerService
    {
        Task InvalidateInventoryCacheAsync();
        Task InvalidateOrderCacheAsync(int? orderId = null);
        Task InvalidateAllCachesAsync();
        Task WarmupCriticalCachesAsync();
    }

    public class CacheManagerService : ICacheManagerService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheManagerService> _logger;

        // Cache key constants to prevent typos and ensure consistency
        private const string INVENTORY_CACHE_KEY = "inventory_items";
        private const string ALL_ORDERS_CACHE_KEY = "all_orders";
        private const string RECENT_ORDERS_CACHE_KEY = "recent_orders";
        private const string ORDER_CACHE_KEY_PREFIX = "order_";

        public CacheManagerService(ICacheService cacheService, ILogger<CacheManagerService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task InvalidateInventoryCacheAsync()
        {
            await _cacheService.RemoveAsync(INVENTORY_CACHE_KEY);
            _logger.LogDebug("Invalidated inventory cache");
        }

        public async Task InvalidateOrderCacheAsync(int? orderId = null)
        {
            await _cacheService.RemoveAsync(ALL_ORDERS_CACHE_KEY);
            await _cacheService.RemoveAsync(RECENT_ORDERS_CACHE_KEY);
            
            if (orderId.HasValue)
            {
                await _cacheService.RemoveAsync($"{ORDER_CACHE_KEY_PREFIX}{orderId.Value}");
            }
            
            _logger.LogDebug("Invalidated order caches for order ID: {OrderId}", orderId ?? 0);
        }

        public async Task InvalidateAllCachesAsync()
        {
            var tasks = new[]
            {
                InvalidateInventoryCacheAsync(),
                InvalidateOrderCacheAsync(),
                _cacheService.RemoveByPatternAsync("order_") // Remove all individual order caches
            };

            await Task.WhenAll(tasks);
            _logger.LogInformation("Invalidated all application caches");
        }

        public async Task WarmupCriticalCachesAsync()
        {
            _logger.LogInformation("Critical cache warmup not implemented in manager - handled by StatePersistenceService");
            // This would require injecting additional services, 
            // so we'll keep it in StatePersistenceService for now
            await Task.CompletedTask;
        }
    }
}