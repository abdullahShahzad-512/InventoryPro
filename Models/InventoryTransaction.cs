using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }   

        [Required]
        public int QuantityChange { get; set; }

        [Required]
        public string TransactionType { get; set; } = string.Empty;

        [Required]
        public decimal TotalPrice { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public string? ReferenceId { get; set; }

        public Product Product { get; set; }
    }
}
