using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class Inventory
    {
        [Key]
        public int ProductId { get; set; }   

        [Required]
        public int CurrentQuantity { get; set; } = 0;

        [Required]
        public decimal AverageCost { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public Product Product { get; set; }
    }
}
