using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class PrayerRequest
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        [Required]
        [StringLength(2000)]
        public string PrayerRequestText { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; } = false;

        // ADD THESE TWO PROPERTIES
        public string? Email { get; set; }
        public string? SubmitterName { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool IsPrayedFor { get; set; } = false;
        public DateTime? PrayedForAt { get; set; }
        public int? PrayedForBy { get; set; }
        public string? ResponseNotes { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<PrayerResponse> Responses { get; set; } = new List<PrayerResponse>();
    }

    public class PrayerResponse
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PrayerRequest")]
        public int PrayerRequestId { get; set; }

        [ForeignKey("Responder")]
        public int ResponderId { get; set; }

        [Required]
        public string ResponseText { get; set; } = string.Empty;

        public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

        public virtual PrayerRequest PrayerRequest { get; set; } = null!;
        public virtual User Responder { get; set; } = null!;
    }
}