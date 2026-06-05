using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using JCAM_CONNECT.Data;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,ChurchLeader")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reports/attendance/csv
        [HttpGet("attendance/csv")]
        public async Task<IActionResult> ExportAttendanceToCsv([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var attendance = await _context.Attendances
                .Include(a => a.Member)
                .ThenInclude(m => m.User)
                .Include(a => a.Event)
                .Where(a => a.AttendedAt >= start && a.AttendedAt <= end)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Member Name,Event Title,Event Date,Attended At,Present");

            foreach (var record in attendance)
            {
                csv.AppendLine($"\"{record.Member.User.FullName}\",\"{record.Event.Title}\",{record.Event.EventDate:yyyy-MM-dd},{record.AttendedAt:yyyy-MM-dd HH:mm},{record.IsPresent}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Attendance_Report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }

        // GET: api/reports/donations/csv
        [HttpGet("donations/csv")]
        public async Task<IActionResult> ExportDonationsToCsv([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var donations = await _context.Donations
                .Include(d => d.User)
                .Where(d => d.DonationDate >= start && d.DonationDate <= end && d.Status == "Completed")
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Donor Name,Donor Email,Amount,Currency,Purpose,Donation Date,Reference");

            foreach (var donation in donations)
            {
                csv.AppendLine($"\"{donation.User.FullName}\",\"{donation.User.Email}\",{donation.Amount},{donation.Currency},\"{donation.Purpose}\",{donation.DonationDate:yyyy-MM-dd},{donation.Reference}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Donations_Report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }

        // GET: api/reports/members/csv
        [HttpGet("members/csv")]
        public async Task<IActionResult> ExportMembersToCsv()
        {
            var members = await _context.Users
                .Include(u => u.MemberProfile)
                .Where(u => u.Role == "Member" && u.IsActive == true)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Full Name,Email,Phone Number,Join Date,Baptism Date,Gender,Occupation,Marital Status");

            foreach (var member in members)
            {
                csv.AppendLine($"\"{member.FullName}\",\"{member.Email}\",\"{member.PhoneNumber ?? ""}\",{member.CreatedAt:yyyy-MM-dd},\"{member.MemberProfile?.BaptismDate:yyyy-MM-dd}\",\"{member.MemberProfile?.Gender ?? ""}\",\"{member.MemberProfile?.Occupation ?? ""}\",\"{member.MemberProfile?.MaritalStatus ?? ""}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Members_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // GET: api/reports/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalMembers = await _context.Users.CountAsync(u => u.Role == "Member" && u.IsActive == true);
            var newMembersThisMonth = await _context.Users
                .CountAsync(u => u.Role == "Member" && u.CreatedAt.Month == DateTime.UtcNow.Month);

            var totalEvents = await _context.Events.CountAsync(e => e.EventDate >= DateTime.UtcNow);
            var totalTestimonies = await _context.Testimonies.CountAsync(t => t.IsApproved == true);
            var pendingTestimonies = await _context.Testimonies.CountAsync(t => t.IsApproved == false);
            var pendingPrayerRequests = await _context.PrayerRequests.CountAsync(p => p.IsPrayedFor == false);

            var totalDonations = await _context.Donations
                .Where(d => d.Status == "Completed")
                .SumAsync(d => (decimal?)d.Amount) ?? 0;

            var monthlyAttendance = await _context.Attendances
                .Where(a => a.AttendedAt.Month == DateTime.UtcNow.Month && a.IsPresent == true)
                .CountAsync();

            return Ok(new
            {
                TotalMembers = totalMembers,
                NewMembersThisMonth = newMembersThisMonth,
                UpcomingEvents = totalEvents,
                PublishedTestimonies = totalTestimonies,
                PendingTestimonies = pendingTestimonies,
                PendingPrayerRequests = pendingPrayerRequests,
                TotalDonations = totalDonations,
                MonthlyAttendance = monthlyAttendance
            });
        }
    }
}