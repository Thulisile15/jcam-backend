using System.ComponentModel.DataAnnotations;

namespace JCAM_CONNECT.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual MemberProfile? MemberProfile { get; set; }
        public virtual ICollection<Testimony> Testimonies { get; set; } = new List<Testimony>();
        public virtual ICollection<PrayerRequest> PrayerRequests { get; set; } = new List<PrayerRequest>();
        public virtual ICollection<CounsellingSession> CounsellingSessionsAsMember { get; set; } = new List<CounsellingSession>();
        public virtual ICollection<CounsellingSession> CounsellingSessionsAsPastor { get; set; } = new List<CounsellingSession>();
        public virtual ICollection<PrayerResponse> PrayerResponses { get; set; } = new List<PrayerResponse>();
        public virtual ICollection<Devotional> Devotionals { get; set; } = new List<Devotional>();
    }
}