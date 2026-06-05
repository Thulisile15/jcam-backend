using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.Models;
using System.Security.Cryptography;
using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Authorization;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/passwordreset/request
        [HttpPost("request")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetRequestDto requestDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == requestDto.Email);

            if (user == null)
                return Ok(new { message = "If your email is registered, you will receive a reset link" });

            // Generate reset token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // In production, send email with reset link
            var resetLink = $"https://yourfrontend.com/reset-password?token={Uri.EscapeDataString(token)}";

            // Log for testing (remove in production)
            Console.WriteLine($"Reset link for {user.Email}: {resetLink}");

            return Ok(new { message = "If your email is registered, you will receive a reset link", resetLink }); // Remove resetLink in production
        }

        // POST: api/passwordreset/reset
        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == resetDto.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null)
                return BadRequest(new { message = "Invalid or expired reset token" });

            // Update password
            resetToken.User.PasswordHash = BCryptNet.HashPassword(resetDto.NewPassword);
            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully" });
        }
    }

    public class ResetRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}