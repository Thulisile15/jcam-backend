using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.DTOs;
using JCAM_CONNECT.Models;
using JCAM_CONNECT.Helpers;
using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Authorization;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(ApplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCryptNet.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = registerDto.Role ?? "Member",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (user.Role != "Admin")
            {
                var memberProfile = new MemberProfile
                {
                    UserId = user.Id,
                    JoinDate = DateTime.UtcNow
                };
                _context.MemberProfiles.Add(memberProfile);
                await _context.SaveChangesAsync();
            }

            var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCryptNet.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is deactivated" });
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token
            });
        }

        // POST: api/auth/register-admin (Only for creating the FIRST admin)
        [HttpPost("register-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto registerDto)
        {
            // Check if any admin already exists (case insensitive)
            var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Role.ToLower() == "admin");
            if (existingAdmin != null)
            {
                return BadRequest(new { message = "Admin already exists. Only regular members can register." });
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Create Admin user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCryptNet.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create MemberProfile for Admin
            var memberProfile = new MemberProfile
            {
                UserId = user.Id,
                JoinDate = DateTime.UtcNow
            };
            _context.MemberProfiles.Add(memberProfile);
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token
            });
        }

        // POST: api/auth/reset-admin-password (Reset admin password)
        [HttpPost("reset-admin-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetAdminPassword([FromBody] ResetAdminPasswordDto resetDto)
        {
            // Find admin user (case insensitive)
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role.ToLower() == "admin");
            if (admin == null)
            {
                return NotFound(new { message = "Admin not found. Please register an admin first." });
            }

            // Hash the new password
            admin.PasswordHash = BCryptNet.HashPassword(resetDto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Password reset successfully for {admin.Email}. You can now login with your new password." });
        }
    }

    // DTO for password reset
    public class ResetAdminPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}