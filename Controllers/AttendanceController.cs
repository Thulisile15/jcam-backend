using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.Models;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/attendance/event/{eventId}
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GetEventAttendance(int eventId)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Member)
                .ThenInclude(m => m.User)
                .Where(a => a.EventId == eventId)
                .Select(a => new
                {
                    a.Id,
                    a.AttendedAt,
                    a.IsPresent,
                    MemberName = a.Member.User.FullName,
                    MemberId = a.Member.UserId
                })
                .ToListAsync();

            return Ok(attendance);
        }

        // GET: api/attendance/member/{memberId}
        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetMemberAttendance(int memberId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId != memberId && !User.IsInRole("Admin") && !User.IsInRole("ChurchLeader"))
                return Forbid();

            var attendance = await _context.Attendances
                .Include(a => a.Event)
                .Where(a => a.MemberId == memberId)
                .OrderByDescending(a => a.AttendedAt)
                .Select(a => new
                {
                    a.Id,
                    a.AttendedAt,
                    a.IsPresent,
                    EventTitle = a.Event.Title,
                    EventDate = a.Event.EventDate
                })
                .ToListAsync();

            return Ok(attendance);
        }

        // POST: api/attendance/mark
        [HttpPost("mark")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto attendanceDto)
        {
            // Check if attendance already exists for this member and event
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.MemberId == attendanceDto.MemberId && a.EventId == attendanceDto.EventId);

            if (existing != null)
            {
                existing.IsPresent = attendanceDto.IsPresent;
                existing.AttendedAt = attendanceDto.AttendedDate ?? DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Attendance updated successfully" });
            }

            var attendance = new Attendance
            {
                MemberId = attendanceDto.MemberId,
                EventId = attendanceDto.EventId,
                AttendedAt = attendanceDto.AttendedDate ?? DateTime.UtcNow,
                IsPresent = attendanceDto.IsPresent
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Attendance marked successfully" });
        }

        // POST: api/attendance/bulk-mark
        [HttpPost("bulk-mark")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> BulkMarkAttendance([FromBody] BulkAttendanceDto bulkDto)
        {
            foreach (var memberId in bulkDto.MemberIds)
            {
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.MemberId == memberId && a.EventId == bulkDto.EventId);

                if (existing != null)
                {
                    existing.IsPresent = true;
                    existing.AttendedAt = bulkDto.AttendedDate ?? DateTime.UtcNow;
                }
                else
                {
                    var attendance = new Attendance
                    {
                        MemberId = memberId,
                        EventId = bulkDto.EventId,
                        AttendedAt = bulkDto.AttendedDate ?? DateTime.UtcNow,
                        IsPresent = true
                    };
                    _context.Attendances.Add(attendance);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Attendance marked for {bulkDto.MemberIds.Count} members" });
        }

        // GET: api/attendance/stats/event/{eventId}
        [HttpGet("stats/event/{eventId}")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GetEventStats(int eventId)
        {
            var totalMembers = await _context.Users.CountAsync(u => u.Role == "Member" && u.IsActive == true);
            var presentCount = await _context.Attendances
                .Where(a => a.EventId == eventId && a.IsPresent == true)
                .CountAsync();

            return Ok(new
            {
                EventId = eventId,
                TotalMembers = totalMembers,
                PresentCount = presentCount,
                AbsentCount = totalMembers - presentCount,
                AttendancePercentage = totalMembers > 0 ? (presentCount * 100.0 / totalMembers) : 0
            });
        }
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