using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTrack.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        
        public DateTime DatePlaced { get; set; }
        
        // Navigation property for the one-to-many relationship
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Legacy property for backward compatibility (not mapped to database)
        [NotMapped]
        public List<InventoryItem> Items
        {
            get
            {
                return OrderItems.Select(oi => oi.InventoryItem).ToList();
            }
        }

        public void AddItem(InventoryItem item, int quantityOrdered = 1)
        {
            if (item != null)
            {
                var orderItem = new OrderItem
                {
                    InventoryItemId = item.ItemId,
                    InventoryItem = item,
                    QuantityOrdered = quantityOrdered
                };
                OrderItems.Add(orderItem);
            }
        }

        public void RemoveItem(int itemId)
        {
            var orderItemToRemove = OrderItems.FirstOrDefault(oi => oi.InventoryItemId == itemId);
            if (orderItemToRemove != null)
            {
                OrderItems.Remove(orderItemToRemove);
            }
        }

        public string GetOrderSummary()
        {
            return $"Order #{OrderId} for {CustomerName} | Items: {OrderItems.Count} | Placed: {DatePlaced:M/d/yyyy}";
        }

        // Efficient method to get order total quantity
        public int GetTotalQuantity()
        {
            return OrderItems.Sum(oi => oi.QuantityOrdered);
        }

        // Efficient method to print order details
        public void PrintOrderDetails()
        {
            Console.WriteLine(GetOrderSummary());
            Console.WriteLine("Order Details:");
            foreach (var orderItem in OrderItems)
            {
                Console.WriteLine($"  - {orderItem.InventoryItem.Name}: {orderItem.QuantityOrdered} units from {orderItem.InventoryItem.Location}");
            }
        }
    }
}