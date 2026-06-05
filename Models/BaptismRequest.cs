using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class BaptismRequest
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        // ADD THESE PROPERTIES
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        public DateTime PreferredDate { get; set; }

        public string? Notes { get; set; }

        public bool RequestCertificate { get; set; } = false;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed, Rejected

        public DateTime? BaptismDate { get; set; }
        public string? BaptismLocation { get; set; }
        public string? BaptismOfficiant { get; set; }
        public DateTime? CertificateSentAt { get; set; }
        public string? CertificateFilePath { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }

        public virtual User? User { get; set; }
    }
}