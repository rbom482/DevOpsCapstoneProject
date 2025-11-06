using LogiTrack.Models;
using LogiTrack.Services;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Services
{
    public interface IStatePersistenceService
    {
        Task<bool> ValidateDatabasePersistenceAsync();
        Task<Dictionary<string, object>> GetSystemStateAsync();
        Task RestoreSystemStateAsync();
        Task<bool> VerifyDataIntegrityAsync();
    }

    public class StatePersistenceService : IStatePersistenceService
    {
        private readonly LogiTrackContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<StatePersistenceService> _logger;

        public StatePersistenceService(
            LogiTrackContext context, 
            ICacheService cacheService,
            ILogger<StatePersistenceService> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<bool> ValidateDatabasePersistenceAsync()
        {
            try
            {
                _logger.LogInformation("Validating database persistence...");

                // Test database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database");
                    return false;
                }

                // Verify critical tables exist and have data
                var inventoryCount = await _context.InventoryItems.CountAsync();
                var orderCount = await _context.Orders.CountAsync();
                var userCount = await _context.Users.CountAsync();

                _logger.LogInformation("Database validation results - Inventory: {InventoryCount}, Orders: {OrderCount}, Users: {UserCount}",
                    inventoryCount, orderCount, userCount);

                // Test write operation to verify persistence
                var testItem = new InventoryItem
                {
                    Name = "PERSISTENCE_TEST_" + Guid.NewGuid().ToString()[..8],
                    Quantity = 1,
                    Location = "TEST_LOCATION"
                };

                _context.InventoryItems.Add(testItem);
                await _context.SaveChangesAsync();

                // Verify the test item persisted
                var persistedItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ItemId == testItem.ItemId);

                if (persistedItem != null)
                {
                    // Clean up test data
                    _context.InventoryItems.Remove(persistedItem);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Database persistence validation successful");
                    return true;
                }

                _logger.LogError("Test item was not persisted to database");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating database persistence");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetSystemStateAsync()
        {
            try
            {
                var state = new Dictionary<string, object>();

                // Database state
                state["DatabaseConnected"] = await _context.Database.CanConnectAsync();
                state["InventoryCount"] = await _context.InventoryItems.CountAsync();
                state["OrderCount"] = await _context.Orders.CountAsync();
                state["UserCount"] = await _context.Users.CountAsync();

                // Cache state
                state["CacheActive"] = await _cacheService.ExistsAsync("inventory_items") ||
                                     await _cacheService.ExistsAsync("all_orders");

                // System info
                state["ServerTime"] = DateTime.UtcNow;
                state["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

                return state;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system state");
                return new Dictionary<string, object> { ["Error"] = ex.Message };
            }
        }

        public async Task RestoreSystemStateAsync()
        {
            try
            {
                _logger.LogInformation("Restoring system state...");

                // Warm up critical caches after restart
                await WarmupCachesAsync();

                // Verify database connections
                await ValidateDatabasePersistenceAsync();

                _logger.LogInformation("System state restoration completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring system state");
                throw;
            }
        }

        public async Task<bool> VerifyDataIntegrityAsync()
        {
            try
            {
                _logger.LogInformation("Verifying data integrity...");

                // Check for orphaned order items
                var orphanedOrderItems = await _context.OrderItems
                    .Where(oi => !_context.Orders.Any(o => o.OrderId == oi.OrderId))
                    .CountAsync();

                if (orphanedOrderItems > 0)
                {
                    _logger.LogWarning("Found {Count} orphaned order items", orphanedOrderItems);
                }

                // Check for orders referencing non-existent inventory items
                var invalidOrderItems = await _context.OrderItems
                    .Where(oi => !_context.InventoryItems.Any(i => i.ItemId == oi.InventoryItemId))
                    .CountAsync();

                if (invalidOrderItems > 0)
                {
                    _logger.LogWarning("Found {Count} order items referencing non-existent inventory", invalidOrderItems);
                }

                // Check for negative quantities
                var negativeInventory = await _context.InventoryItems
                    .Where(i => i.Quantity < 0)
                    .CountAsync();

                if (negativeInventory > 0)
                {
                    _logger.LogWarning("Found {Count} inventory items with negative quantities", negativeInventory);
                }

                var integrityIssues = orphanedOrderItems + invalidOrderItems + negativeInventory;
                _logger.LogInformation("Data integrity check completed. Issues found: {Count}", integrityIssues);

                return integrityIssues == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying data integrity");
                return false;
            }
        }

        private async Task WarmupCachesAsync()
        {
            try
            {
                // Preload frequently accessed data into cache
                _logger.LogInformation("Warming up caches...");

                // Warm up inventory cache
                var inventoryItems = await _context.InventoryItems.AsNoTracking().ToListAsync();
                await _cacheService.SetAsync("inventory_items", inventoryItems, TimeSpan.FromHours(1));

                // Warm up recent orders cache
                var recentOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.InventoryItem)
                    .Where(o => o.DatePlaced >= DateTime.UtcNow.AddDays(-30))
                    .AsNoTracking()
                    .ToListAsync();

                await _cacheService.SetAsync("recent_orders", recentOrders, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Cache warmup completed - Inventory: {InventoryCount}, Recent Orders: {OrderCount}",
                    inventoryItems.Count, recentOrders.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up caches");
            }
        }
    }
}