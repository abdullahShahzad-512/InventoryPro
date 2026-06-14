using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{
    public class ProductSupplier
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public decimal CostPrice { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
