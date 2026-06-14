using System.ComponentModel.DataAnnotations;
namespace IMS.Models
{

    public class Supplier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Supplier name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string SupplierName { get; set; }

        [Required(ErrorMessage = "Supplier contact is required.")]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(14)]
        public string ContactNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(120)]
        public string Email { get; set; }

        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }


}
