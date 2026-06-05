using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.Models;
using JCAM_CONNECT.Services;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CounsellingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public CounsellingController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var pendingSessions = await _context.CounsellingSessions
                .Where(c => c.Status == "Pending" || c.Status == null)
                .OrderBy(c => c.RequestedDate)
                .Select(c => new
                {
                    c.Id,
                    c.FullName,
                    c.Email,
                    c.PhoneNumber,
                    c.RequestedDate,
                    c.PreferredDate,
                    c.Notes,
                    c.Status
                })
                .ToListAsync();

            return Ok(pendingSessions);
        }

        // FIXED: Added [HttpGet("all")] to match frontend
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSessions()
        {
            var sessions = await _context.CounsellingSessions
                .OrderByDescending(c => c.RequestedDate)
                .Select(c => new
                {
                    c.Id,
                    c.FullName,
                    c.Email,
                    c.PhoneNumber,
                    c.RequestedDate,
                    c.PreferredDate,
                    c.ScheduledDate,
                    c.Status,
                    c.Notes,
                    c.CounsellorNotes
                })
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpPost]
        public async Task<IActionResult> RequestSession([FromBody] CounsellingRequestDto requestDto)
        {
            var session = new CounsellingSession
            {
                FullName = requestDto.FullName,
                Email = requestDto.Email,
                PhoneNumber = requestDto.PhoneNumber,
                RequestedDate = DateTime.UtcNow,
                PreferredDate = requestDto.PreferredDate,
                Notes = requestDto.Notes,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.CounsellingSessions.Add(session);
            await _context.SaveChangesAsync();

            // EMAIL CODE ADDED - Send confirmation email to requester
            if (!string.IsNullOrEmpty(session.Email))
            {
                await _emailService.SendEmailAsync(
                    session.Email,
                    "Your Counselling Request Has Been Received - JCAM Ministries",
                    _emailService.GetCounsellingReceivedTemplate(session.FullName ?? "Beloved")
                );
            }

            return Ok(new { message = "Counselling session requested successfully", sessionId = session.Id });
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveSession(int id, [FromBody] ApproveCounsellingDto approveDto)
        {
            var session = await _context.CounsellingSessions.FindAsync(id);
            if (session == null)
                return NotFound(new { message = "Session not found" });

            session.ScheduledDate = approveDto.ScheduledDate;
            session.Status = "Approved";

            await _context.SaveChangesAsync();

            // EMAIL CODE ADDED - Send approval email with scheduled date
            if (!string.IsNullOrEmpty(session.Email))
            {
                await _emailService.SendEmailAsync(
                    session.Email,
                    "Your Counselling Session Has Been Scheduled - JCAM Ministries",
                    _emailService.GetCounsellingApprovedTemplate(session.FullName ?? "Beloved", session.ScheduledDate.Value)
                );
            }

            return Ok(new { message = "Session approved successfully" });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectSession(int id)
        {
            var session = await _context.CounsellingSessions.FindAsync(id);
            if (session == null)
                return NotFound(new { message = "Session not found" });

            session.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session rejected" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var session = await _context.CounsellingSessions.FindAsync(id);
            if (session == null)
                return NotFound(new { message = "Session not found" });

            _context.CounsellingSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session deleted successfully" });
        }
    }

    public class CounsellingRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ApproveCounsellingDto
    {
        public DateTime ScheduledDate { get; set; }
    }
}