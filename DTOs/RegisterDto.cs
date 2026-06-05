using System.ComponentModel.DataAnnotations;

namespace JCAM_CONNECT.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Role { get; set; } = "Member";
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

   

    
    public class AddMemberDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? MaritalStatus { get; set; }
    }

    public class UpdateMemberDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? MaritalStatus { get; set; }
    }

    public class CreateDevotionalDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? BibleVerse { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    public class UpdateDevotionalDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? BibleVerse { get; set; }
        public bool? IsPublished { get; set; }
    }

    public class RequestSessionDto
    {
        public DateTime? PreferredDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ApproveSessionDto
    {
        public DateTime ScheduledDate { get; set; }
    }

    public class RecordBaptismDto
    {
        public int MemberId { get; set; }
        public DateTime BaptismDate { get; set; }
        public string BaptismLocation { get; set; } = string.Empty;
        public string BaptismOfficiant { get; set; } = string.Empty;
    }

    public class MarkAttendanceDto
    {
        public int MemberId { get; set; }
        public int EventId { get; set; }
        public bool IsPresent { get; set; } = true;
        public DateTime? AttendedDate { get; set; }
    }

    public class BulkAttendanceDto
    {
        public List<int> MemberIds { get; set; } = new List<int>();
        public int EventId { get; set; }
        public DateTime? AttendedDate { get; set; }
    }
}