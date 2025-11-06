using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using LogiTrack.Models;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;

        public InventoryController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventoryItems()
        {
            const string cacheKey = "inventory_items";
            var stopwatch = Stopwatch.StartNew();

            // Try to get items from cache first
            if (_cache.TryGetValue(cacheKey, out List<InventoryItem>? cachedItems))
            {
                stopwatch.Stop();
                Response.Headers["X-Cache-Status"] = "HIT";
                Response.Headers["X-Response-Time"] = $"{stopwatch.ElapsedMilliseconds}ms";
                return Ok(cachedItems);
            }

            // If not in cache, fetch from database
            var items = await _context.InventoryItems
                .AsNoTracking() // Optimize for read-only scenarios
                .ToListAsync();

            // Cache the result for 30 seconds
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(10),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, items, cacheOptions);

            stopwatch.Stop();
            Response.Headers["X-Cache-Status"] = "MISS";
            Response.Headers["X-Response-Time"] = $"{stopwatch.ElapsedMilliseconds}ms";
            
            return Ok(items);
        }

        // GET: api/inventory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItem>> GetInventoryItem(int id)
        {
            var item = await _context.InventoryItems
                .AsNoTracking() // Optimize for read-only scenarios
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
            {
                throw new KeyNotFoundException($"Inventory item with ID {id} not found");
            }

            return Ok(item);
        }

        // POST: api/inventory
        [HttpPost]
        public async Task<ActionResult<InventoryItem>> CreateInventoryItem(InventoryItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for duplicate names in the same location
            var existingItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Name == item.Name && i.Location == item.Location);
                
            if (existingItem != null)
            {
                throw new ArgumentException($"An item named '{item.Name}' already exists in location '{item.Location}'");
            }

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            // Invalidate cache after creation
            InvalidateInventoryCache();

            return CreatedAtAction(
                nameof(GetInventoryItem), 
                new { id = item.ItemId }, 
                item);
        }

        // PUT: api/inventory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem(int id, InventoryItem item)
        {
            if (id != item.ItemId)
            {
                throw new ArgumentException("ID in URL does not match ID in request body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await InventoryItemExistsAsync(id))
            {
                throw new KeyNotFoundException($"Inventory item with ID {id} not found");
            }

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Invalidate cache after update
            InvalidateInventoryCache();

            return NoContent();
        }

        // DELETE: api/inventory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Inventory item with ID {id} not found");
            }

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            // Invalidate cache after deletion
            InvalidateInventoryCache();

            return NoContent();
        }

        /// <summary>
        /// Invalidates the inventory cache when data is modified
        /// </summary>
        private void InvalidateInventoryCache()
        {
            _cache.Remove("inventory_items");
        }

        private async Task<bool> InventoryItemExistsAsync(int id)
        {
            return await _context.InventoryItems.AnyAsync(e => e.ItemId == id);
        }
    }
}