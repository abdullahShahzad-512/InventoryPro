using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; }

        [Required]
        [StringLength(50)]
        public string Barcode { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }
        
        [Required]
        [Range(0, 50)]
        public int ReorderLevel { get; set; }

        [Required]
        [Range( 0, double.MaxValue)]
        public decimal SalePrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
