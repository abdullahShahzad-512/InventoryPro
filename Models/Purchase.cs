using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class Purchase
    {
       
            public int Id { get; set; }

        [Required]
            public int SupplierId { get; set; }

            public Supplier Supplier { get; set; } = null!;

            public int NumberOfProducts { get; set; }
            public DateTime PurchaseDate { get; set; } = DateTime.Now;

            public decimal TotalExpectedAmount { get; set; } 
        [Required]
            public decimal TotalPaidAmount { get; set; }

            public decimal DiscountAmount => TotalExpectedAmount - TotalPaidAmount;

            public string? Notes { get; set; }
        [Required]
            public List<PurchaseProduct> PurchaseProducts { get; set; } = new List<PurchaseProduct>();
    }

    
}
