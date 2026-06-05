using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.Models;
using JCAM_CONNECT.Services;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaptismController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public BaptismController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var requests = await _context.BaptismRequests
                .Where(b => b.Status == "Pending")
                .OrderBy(b => b.RequestedAt)
                .Select(b => new
                {
                    b.Id,
                    b.FullName,
                    b.Email,
                    b.PhoneNumber,
                    b.PreferredDate,
                    b.Notes,
                    b.RequestCertificate,
                    b.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _context.BaptismRequests
                .OrderByDescending(b => b.RequestedAt)
                .Select(b => new
                {
                    b.Id,
                    b.FullName,
                    b.Email,
                    b.PhoneNumber,
                    b.PreferredDate,
                    b.Status,
                    b.BaptismDate,
                    b.BaptismLocation,
                    b.BaptismOfficiant,
                    b.RequestCertificate,
                    b.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestBaptism([FromBody] BaptismRequestDto requestDto)
        {
            var baptismRequest = new BaptismRequest
            {
                FullName = requestDto.FullName,
                Email = requestDto.Email,
                PhoneNumber = requestDto.PhoneNumber,
                PreferredDate = requestDto.PreferredDate,
                Notes = requestDto.Notes,
                RequestCertificate = requestDto.RequestCertificate,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _context.BaptismRequests.Add(baptismRequest);
            await _context.SaveChangesAsync();

            // Send confirmation email
            if (!string.IsNullOrEmpty(baptismRequest.Email))
            {
                await _emailService.SendEmailAsync(
                    baptismRequest.Email,
                    "Your Baptism Request Has Been Received - JCAM Ministries",
                    _emailService.GetBaptismReceivedTemplate(baptismRequest.FullName)
                );
            }

            return Ok(new { message = "Baptism request submitted successfully!", requestId = baptismRequest.Id });
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveBaptism(int id, [FromBody] ApproveBaptismDto approveDto)
        {
            var request = await _context.BaptismRequests.FindAsync(id);
            if (request == null)
                return NotFound(new { message = "Baptism request not found" });

            request.Status = "Approved";
            request.BaptismDate = approveDto.BaptismDate;
            request.BaptismLocation = approveDto.BaptismLocation;
            request.BaptismOfficiant = approveDto.BaptismOfficiant;
            request.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send approval email
            if (!string.IsNullOrEmpty(request.Email))
            {
                await _emailService.SendEmailAsync(
                    request.Email,
                    "Your Baptism Has Been Approved - JCAM Ministries",
                    _emailService.GetBaptismApprovedTemplate(request.FullName, request.BaptismDate.Value, request.BaptismLocation ?? "JCAM Ministries")
                );
            }

            return Ok(new { message = "Baptism approved successfully!" });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectBaptism(int id)
        {
            var request = await _context.BaptismRequests.FindAsync(id);
            if (request == null)
                return NotFound(new { message = "Baptism request not found" });

            request.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Baptism request rejected" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBaptism(int id)
        {
            var request = await _context.BaptismRequests.FindAsync(id);
            if (request == null)
                return NotFound(new { message = "Baptism request not found" });

            _context.BaptismRequests.Remove(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Baptism request deleted successfully" });
        }
    }

    public class BaptismRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime PreferredDate { get; set; }
        public string? Notes { get; set; }
        public bool RequestCertificate { get; set; } = false;
    }

    public class ApproveBaptismDto
    {
        public DateTime BaptismDate { get; set; }
        public string BaptismLocation { get; set; } = string.Empty;
        public string BaptismOfficiant { get; set; } = string.Empty;
    }
}