using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "ZAR";

        public string Purpose { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string Reference { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime DonationDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}