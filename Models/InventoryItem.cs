using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Models
{
    [Index(nameof(Name))]
    [Index(nameof(Location))]
    public class InventoryItem
    {
        [Key]
        public int ItemId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public int Quantity { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Location { get; set; } = string.Empty;

        public void DisplayInfo()
        {
            Console.WriteLine($"Item: {Name} | Quantity: {Quantity} | Location: {Location}");
        }

        // Performance improvement: Override ToString for better debugging
        public override string ToString()
        {
            return $"{Name} (ID: {ItemId}, Qty: {Quantity}, Location: {Location})";
        }
    }
}