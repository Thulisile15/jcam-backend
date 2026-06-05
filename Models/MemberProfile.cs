using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class MemberProfile
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? MaritalStatus { get; set; }
        public DateTime? JoinDate { get; set; }

        public DateTime? BaptismDate { get; set; }
        public string? BaptismLocation { get; set; }
        public string? BaptismOfficiant { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<CounsellingSession> CounsellingSessions { get; set; } = new List<CounsellingSession>();
        public virtual ICollection<BaptismRequest> BaptismRequests { get; set; } = new List<BaptismRequest>(); // ADD THIS LINE
    }
}