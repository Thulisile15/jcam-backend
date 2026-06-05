namespace JCAM_CONNECT.DTOs
{
    public class PrayerRequestDto
    {
        public string PrayerRequestText { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; } = false;
        public string? Email { get; set; }
        public string? SubmitterName { get; set; }
    }
}