using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        public int InventoryItemId { get; set; }
        public InventoryItem InventoryItem { get; set; } = null!;
        
        public int QuantityOrdered { get; set; }
    }
}