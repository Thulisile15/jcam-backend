using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class Testimony
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        public string? Title { get; set; }

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? SubmitterName { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }

        public virtual ICollection<TestimonyDocument> Documents { get; set; } = new List<TestimonyDocument>();
        public virtual User? User { get; set; }
    }

    public class TestimonyDocument
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Testimony")]
        public int TestimonyId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public virtual Testimony Testimony { get; set; } = null!;
    }
}