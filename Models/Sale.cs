
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        [StringLength(50)]
        public string UserName { get; set; }

        [ForeignKey("CustomerId")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Completed"; // Completed, Pending, Cancelled

        [StringLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; } = 0;

        // Navigation property
        public virtual List<SaleProducts> SaleProducts { get; set; } = new List<SaleProducts>();
    }

}