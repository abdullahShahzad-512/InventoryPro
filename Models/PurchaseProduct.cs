using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class PurchaseProduct
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public Purchase Purchase { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public decimal CostPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => CostPrice * Quantity;
    }
}
