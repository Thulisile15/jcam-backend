using Microsoft.AspNetCore.Mvc;
using JCAM_CONNECT.Data;
using Microsoft.EntityFrameworkCore;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginDto loginDto)
        {
            var adminPassword = _configuration["AdminSettings:Password"];
            if (loginDto.Password == adminPassword)
            {
                // Simple session or just return success
                // For simplicity, we'll just return a success flag
                // The frontend will store a simple flag in localStorage
                return Ok(new { success = true, message = "Admin login successful" });
            }
            return Unauthorized(new { success = false, message = "Invalid password" });
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var pendingTestimonies = await _context.Testimonies.CountAsync(t => t.IsApproved == false);
            var pendingPrayers = await _context.PrayerRequests.CountAsync(p => p.IsPrayedFor == false);
            var pendingBaptisms = await _context.BaptismRequests.CountAsync(b => b.Status == "Pending");
            var pendingCounselling = await _context.CounsellingSessions.CountAsync(c => c.Status == "Pending");
            var totalEvents = await _context.Events.CountAsync();

            return Ok(new
            {
                pendingTestimonies,
                pendingPrayers,
                pendingBaptisms,
                pendingCounselling,
                totalEvents
            });
        }
    }

    public class AdminLoginDto
    {
        public string Password { get; set; } = string.Empty;
    }
}