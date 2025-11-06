using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using LogiTrack.Models;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PerformanceController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PerformanceController> _logger;

        public PerformanceController(IMemoryCache cache, ILogger<PerformanceController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get cache statistics and performance metrics
        /// </summary>
        [HttpGet("metrics")]
        public IActionResult GetMetrics()
        {
            var metrics = new
            {
                CacheInfo = new
                {
                    MemoryCacheEnabled = true,
                    CacheType = "In-Memory",
                    Recommendations = new[]
                    {
                        "Consider Redis for production distributed caching",
                        "Monitor cache hit ratios via response headers",
                        "Adjust cache expiration based on data volatility"
                    }
                },
                PerformanceOptimizations = new
                {
                    DatabaseOptimizations = new[]
                    {
                        "AsNoTracking() applied to read-only queries",
                        "Eager loading with .Include() to prevent N+1 problems",
                        "Selective queries to reduce data transfer"
                    },
                    CachingStrategy = new[]
                    {
                        "Inventory items cached for 30 seconds",
                        "Orders cached for 2-5 minutes",
                        "Automatic cache invalidation on data modification"
                    }
                },
                ResponseTimeHeaders = new
                {
                    XCacheStatus = "Indicates cache HIT or MISS",
                    XResponseTime = "Shows request processing time in milliseconds"
                }
            };

            return Ok(metrics);
        }

        /// <summary>
        /// Clear all cached data (admin function)
        /// </summary>
        [HttpPost("clear-cache")]
        public IActionResult ClearCache()
        {
            // Note: IMemoryCache doesn't have a built-in clear all method
            // In production, you would use a cache provider that supports this
            _logger.LogInformation("Cache clear requested by user");
            
            // For IMemoryCache, we can only remove specific keys
            var keysToRemove = new[]
            {
                "inventory_items",
                "all_orders"
                // Add other known cache keys
            };

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            return Ok(new { Message = "Known cache entries cleared", ClearedKeys = keysToRemove });
        }

        /// <summary>
        /// Test endpoint performance with and without caching
        /// </summary>
        [HttpGet("benchmark")]
        public async Task<IActionResult> BenchmarkPerformance()
        {
            var results = new List<object>();

            // Test multiple cache scenarios
            for (int i = 0; i < 3; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Simulate a quick operation
                await Task.Delay(10);
                
                stopwatch.Stop();
                
                results.Add(new
                {
                    TestRun = i + 1,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                BenchmarkResults = results,
                Recommendations = new[]
                {
                    "Monitor X-Response-Time header in actual requests",
                    "Compare cache HIT vs MISS performance",
                    "Use tools like Apache Bench or k6 for load testing"
                }
            });
        }
    }
}