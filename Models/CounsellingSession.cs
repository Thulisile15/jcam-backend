using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class CounsellingSession
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Member")]
        public int? MemberId { get; set; }

        [ForeignKey("Pastor")]
        public int? PastorId { get; set; }

        // ADD THESE PROPERTIES
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? PreferredDate { get; set; }

        public DateTime RequestedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? CounsellorNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("MemberId")]
        public virtual User? Member { get; set; }

        [ForeignKey("PastorId")]
        public virtual User? Pastor { get; set; }
    }
}