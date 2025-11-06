using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LogiTrack.Services;
using System.Diagnostics;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly IStatePersistenceService _statePersistenceService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SystemController> _logger;

        public SystemController(
            IStatePersistenceService statePersistenceService,
            ICacheService cacheService,
            ILogger<SystemController> logger)
        {
            _statePersistenceService = statePersistenceService;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive system health status
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var systemState = await _statePersistenceService.GetSystemStateAsync();
                var databasePersistence = await _statePersistenceService.ValidateDatabasePersistenceAsync();
                var dataIntegrity = await _statePersistenceService.VerifyDataIntegrityAsync();

                stopwatch.Stop();

                var health = new
                {
                    Status = databasePersistence && dataIntegrity ? "Healthy" : "Warning",
                    Timestamp = DateTime.UtcNow,
                    ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                    DatabasePersistence = databasePersistence,
                    DataIntegrity = dataIntegrity,
                    SystemState = systemState,
                    Recommendations = new List<string>()
                };

                if (!databasePersistence)
                {
                    ((List<string>)health.Recommendations).Add("Database persistence issues detected - check connectivity");
                }

                if (!dataIntegrity)
                {
                    ((List<string>)health.Recommendations).Add("Data integrity issues found - run data cleanup procedures");
                }

                if (databasePersistence && dataIntegrity)
                {
                    ((List<string>)health.Recommendations).Add("System is operating normally");
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error checking system health");

                return Ok(new
                {
                    Status = "Error",
                    Timestamp = DateTime.UtcNow,
                    ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                    Error = ex.Message,
                    Recommendations = new[] { "System health check failed - investigate logs" }
                });
            }
        }

        /// <summary>
        /// Restore system state after restart
        /// </summary>
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreSystemState()
        {
            try
            {
                _logger.LogInformation("Manual system state restoration requested");
                await _statePersistenceService.RestoreSystemStateAsync();

                return Ok(new
                {
                    Status = "Success",
                    Message = "System state restored successfully",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring system state");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Failed to restore system state",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Validate database persistence manually
        /// </summary>
        [HttpGet("validate-persistence")]
        public async Task<IActionResult> ValidatePersistence()
        {
            try
            {
                var isValid = await _statePersistenceService.ValidateDatabasePersistenceAsync();

                return Ok(new
                {
                    DatabasePersistence = isValid,
                    Status = isValid ? "Valid" : "Invalid",
                    Message = isValid ? "Database persistence validated successfully" : "Database persistence validation failed",
                    Timestamp = DateTime.UtcNow,
                    Recommendations = isValid 
                        ? new[] { "Database is persisting data correctly" }
                        : new[] { "Check database connectivity and permissions", "Verify disk space availability" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating persistence");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Persistence validation failed",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get detailed cache statistics
        /// </summary>
        [HttpGet("cache-stats")]
        public async Task<IActionResult> GetCacheStatistics()
        {
            try
            {
                var stats = new
                {
                    CacheType = "Enhanced In-Memory Cache",
                    Features = new[]
                    {
                        "Automatic expiration with sliding window",
                        "Pattern-based cache invalidation",
                        "Comprehensive logging and monitoring",
                        "Cache warmup on system startup"
                    },
                    CacheKeys = new
                    {
                        InventoryItems = await _cacheService.ExistsAsync("inventory_items"),
                        AllOrders = await _cacheService.ExistsAsync("all_orders"),
                        RecentOrders = await _cacheService.ExistsAsync("recent_orders")
                    },
                    Configuration = new
                    {
                        InventoryCache = "1 hour absolute, 30 minute sliding",
                        OrdersCache = "2 minutes absolute, 1 minute sliding",
                        IndividualOrderCache = "5 minutes absolute, 2 minute sliding"
                    },
                    Recommendations = new[]
                    {
                        "Monitor cache hit ratios via X-Cache-Status headers",
                        "Consider Redis for production distributed caching",
                        "Adjust expiration times based on data volatility patterns"
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Failed to get cache statistics",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Warmup system caches
        /// </summary>
        [HttpPost("warmup-cache")]
        public async Task<IActionResult> WarmupCache()
        {
            try
            {
                _logger.LogInformation("Manual cache warmup requested");
                await _statePersistenceService.RestoreSystemStateAsync();

                return Ok(new
                {
                    Status = "Success",
                    Message = "Cache warmup completed successfully",
                    Timestamp = DateTime.UtcNow,
                    CachedItems = new[]
                    {
                        "Inventory items (1 hour cache)",
                        "Recent orders (30 minute cache)"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up cache");

                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Cache warmup failed",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}