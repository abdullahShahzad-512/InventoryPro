using System.ComponentModel.DataAnnotations;

namespace IMS.Models
{ 
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CustomerEmail { get; set; }
        
        public int TotalPurchases { get; set; } = 0;


        public DateTime LatestVisited { get; set; } = DateTime.Now;
    }
}
