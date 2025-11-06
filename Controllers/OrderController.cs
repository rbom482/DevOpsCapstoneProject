using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LogiTrack.Models;
using LogiTrack.DTOs;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public OrderController(LogiTrackContext context)
        {
            _context = context;
        }

        // GET: api/order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .ToListAsync();

            var orderDtos = orders.Select(order => MapToOrderResponseDto(order)).ToList();
            return Ok(orderDtos);
        }

        // GET: api/order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found");
            }

            var orderDto = MapToOrderResponseDto(order);
            return Ok(orderDto);
        }

        // POST: api/order
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(createOrderDto.CustomerName))
            {
                throw new ArgumentException("Customer name is required");
            }

            if (createOrderDto.Items == null || !createOrderDto.Items.Any())
            {
                throw new ArgumentException("At least one item must be included in the order");
            }

            // Validate that all inventory items exist and have sufficient quantity
            await ValidateOrderItemsAsync(createOrderDto.Items);

            // Create the order
            var order = new Order
            {
                CustomerName = createOrderDto.CustomerName.Trim(),
                DatePlaced = createOrderDto.DatePlaced ?? DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get the OrderId

            // Create order items
            var orderItems = createOrderDto.Items.Select(item => new OrderItem
            {
                OrderId = order.OrderId,
                InventoryItemId = item.InventoryItemId,
                QuantityOrdered = item.QuantityOrdered
            }).ToList();

            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();

            // Reload the order with all related data
            var createdOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .FirstAsync(o => o.OrderId == order.OrderId);

            var responseDto = MapToOrderResponseDto(createdOrder);

            return CreatedAtAction(
                nameof(GetOrder), 
                new { id = createdOrder.OrderId }, 
                responseDto);
        }

        // DELETE: api/order/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found");
            }

            // Remove all order items first (cascade delete should handle this, but being explicit)
            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/order/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, CreateOrderDto updateOrderDto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(updateOrderDto.CustomerName))
            {
                throw new ArgumentException("Customer name is required");
            }

            // Validate that all inventory items exist
            await ValidateOrderItemsAsync(updateOrderDto.Items);

            // Update order properties
            order.CustomerName = updateOrderDto.CustomerName.Trim();
            if (updateOrderDto.DatePlaced.HasValue)
            {
                order.DatePlaced = updateOrderDto.DatePlaced.Value;
            }

            // Remove existing order items and add new ones
            _context.OrderItems.RemoveRange(order.OrderItems);

            if (updateOrderDto.Items != null && updateOrderDto.Items.Any())
            {
                var newOrderItems = updateOrderDto.Items.Select(item => new OrderItem
                {
                    OrderId = order.OrderId,
                    InventoryItemId = item.InventoryItemId,
                    QuantityOrdered = item.QuantityOrdered
                }).ToList();

                _context.OrderItems.AddRange(newOrderItems);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task ValidateOrderItemsAsync(List<OrderItemDto> items)
        {
            if (items == null || !items.Any())
            {
                return;
            }

            var inventoryItemIds = items.Select(i => i.InventoryItemId).ToList();
            var existingItems = await _context.InventoryItems
                .Where(item => inventoryItemIds.Contains(item.ItemId))
                .ToListAsync();

            if (existingItems.Count != inventoryItemIds.Count)
            {
                var missingIds = inventoryItemIds.Except(existingItems.Select(i => i.ItemId));
                throw new ArgumentException($"The following inventory item IDs were not found: {string.Join(", ", missingIds)}");
            }

            // Validate quantities are positive
            var invalidItems = items.Where(i => i.QuantityOrdered <= 0).ToList();
            if (invalidItems.Any())
            {
                throw new ArgumentException("All item quantities must be greater than zero");
            }
        }

        private static OrderResponseDto MapToOrderResponseDto(Order order)
        {
            return new OrderResponseDto
            {
                OrderId = order.OrderId,
                CustomerName = order.CustomerName,
                DatePlaced = order.DatePlaced,
                TotalItems = order.OrderItems.Count,
                TotalQuantity = order.OrderItems.Sum(oi => oi.QuantityOrdered),
                Items = order.OrderItems.Select(oi => new OrderItemDetailDto
                {
                    OrderItemId = oi.OrderItemId,
                    InventoryItemId = oi.InventoryItemId,
                    ItemName = oi.InventoryItem.Name,
                    Location = oi.InventoryItem.Location,
                    QuantityOrdered = oi.QuantityOrdered
                }).ToList()
            };
        }
    }
}