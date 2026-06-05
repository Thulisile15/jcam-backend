using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Drawing;
using System.Drawing.Imaging;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.Models;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QrController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/qr/event/{eventId}
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GenerateEventQrCode(int eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            // Generate QR code data (URL to check-in endpoint)
            var qrData = $"https://{Request.Host}/api/qr/checkin?eventId={eventId}";

            // Generate QR code image
            using var ms = new MemoryStream();
            using var bitmap = GenerateQrBitmap(qrData);
            bitmap.Save(ms, ImageFormat.Png);

            return File(ms.ToArray(), "image/png", $"event_{eventId}_qrcode.png");
        }

        // POST: api/qr/checkin
        [HttpPost("checkin")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckIn([FromQuery] int eventId, [FromQuery] int? memberId)
        {
            // For anonymous check-in, we need memberId
            // For app-based check-in, use JWT token

            int userId;
            if (memberId.HasValue)
            {
                userId = memberId.Value;
            }
            else
            {
                // Get from JWT token if logged in
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Please login or provide member ID" });
                userId = int.Parse(userIdClaim);
            }

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null)
                return NotFound(new { message = "Member not found" });

            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.MemberId == member.Id && a.EventId == eventId);

            if (existing != null)
            {
                existing.IsPresent = true;
                existing.AttendedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Check-in updated", alreadyCheckedIn = true });
            }

            var attendance = new Attendance
            {
                MemberId = member.Id,
                EventId = eventId,
                AttendedAt = DateTime.UtcNow,
                IsPresent = true
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Check-in successful", isFirstTime = true });
        }

        private Bitmap GenerateQrBitmap(string data)
        {
            // Simple QR code generation - you'll need QRCoder library
            // Install-Package QRCoder

            // For now, return a simple bitmap
            var bitmap = new Bitmap(300, 300);
            using var g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            g.DrawString("QR Code", new Font("Arial", 20), Brushes.Black, new PointF(80, 130));
            return bitmap;
        }
    }
}