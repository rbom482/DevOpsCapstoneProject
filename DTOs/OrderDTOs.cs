using System.ComponentModel.DataAnnotations;

namespace LogiTrack.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 100 characters")]
        public string CustomerName { get; set; } = string.Empty;
        
        public DateTime? DatePlaced { get; set; }
        
        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "At least one item must be included in the order")]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        [Required(ErrorMessage = "Inventory item ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Inventory item ID must be a positive number")]
        public int InventoryItemId { get; set; }
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int QuantityOrdered { get; set; }
    }

    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DatePlaced { get; set; }
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public List<OrderItemDetailDto> Items { get; set; } = new List<OrderItemDetailDto>();
    }

    public class OrderItemDetailDto
    {
        public int OrderItemId { get; set; }
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int QuantityOrdered { get; set; }
    }
}