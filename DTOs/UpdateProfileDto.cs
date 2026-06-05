namespace JCAM_CONNECT.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? MaritalStatus { get; set; }
    }
}