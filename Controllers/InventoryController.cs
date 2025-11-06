using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LogiTrack.Models;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public InventoryController(LogiTrackContext context)
        {
            _context = context;
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventoryItems()
        {
            var items = await _context.InventoryItems.ToListAsync();
            return Ok(items);
        }

        // GET: api/inventory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItem>> GetInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);

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

            return NoContent();
        }

        private async Task<bool> InventoryItemExistsAsync(int id)
        {
            return await _context.InventoryItems.AnyAsync(e => e.ItemId == id);
        }
    }
}