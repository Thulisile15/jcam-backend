using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Member")]
        public int MemberId { get; set; }

        [ForeignKey("Event")]
        public int EventId { get; set; }

        public DateTime AttendedAt { get; set; } = DateTime.UtcNow;
        public bool IsPresent { get; set; } = true;

        public virtual MemberProfile Member { get; set; } = null!;
        public virtual Event Event { get; set; } = null!;
    }
}