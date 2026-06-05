using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.Models;
using JCAM_CONNECT.DTOs;  // Add this line

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/members (Admin/ChurchLeader only)
        [HttpGet]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GetAllMembers()
        {
            var members = await _context.Users
                .Include(u => u.MemberProfile)
                .Where(u => u.Role == "Member")
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.PhoneNumber,
                    u.CreatedAt,
                    u.IsActive,
                    MemberProfile = u.MemberProfile
                })
                .ToListAsync();

            return Ok(members);
        }

        // GET: api/members/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GetMemberById(int id)
        {
            var member = await _context.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == "Member");

            if (member == null)
                return NotFound(new { message = "Member not found" });

            return Ok(member);
        }

        // GET: api/members/profile (logged in member views their own profile)
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var member = await _context.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (member == null)
                return NotFound(new { message = "Profile not found" });

            return Ok(member);
        }

        // POST: api/members (Admin adds a new member)
        [HttpPost]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> AddMember([FromBody] AddMemberDto memberDto)
        {
            // Check if user exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == memberDto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already exists" });

            var user = new User
            {
                Email = memberDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(memberDto.Password),
                FullName = memberDto.FullName,
                PhoneNumber = memberDto.PhoneNumber,
                Address = memberDto.Address,
                Role = "Member",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var memberProfile = new MemberProfile
            {
                UserId = user.Id,
                DateOfBirth = memberDto.DateOfBirth,
                Gender = memberDto.Gender,
                Occupation = memberDto.Occupation,
                MaritalStatus = memberDto.MaritalStatus,
                JoinDate = DateTime.UtcNow
            };

            _context.MemberProfiles.Add(memberProfile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Member added successfully", memberId = user.Id });
        }

        // PUT: api/members/{id} (Admin/ChurchLeader updates member)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> UpdateMember(int id, [FromBody] UpdateMemberDto memberDto)
        {
            var user = await _context.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == "Member");

            if (user == null)
                return NotFound(new { message = "Member not found" });

            user.FullName = memberDto.FullName ?? user.FullName;
            user.PhoneNumber = memberDto.PhoneNumber ?? user.PhoneNumber;
            user.Address = memberDto.Address ?? user.Address;
            user.IsActive = memberDto.IsActive ?? user.IsActive;

            if (user.MemberProfile != null)
            {
                user.MemberProfile.DateOfBirth = memberDto.DateOfBirth ?? user.MemberProfile.DateOfBirth;
                user.MemberProfile.Gender = memberDto.Gender ?? user.MemberProfile.Gender;
                user.MemberProfile.Occupation = memberDto.Occupation ?? user.MemberProfile.Occupation;
                user.MemberProfile.MaritalStatus = memberDto.MaritalStatus ?? user.MemberProfile.MaritalStatus;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Member updated successfully" });
        }

        // PUT: api/members/profile (Member updates their own profile)
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto profileDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update basic info
            user.FullName = profileDto.FullName ?? user.FullName;
            user.PhoneNumber = profileDto.PhoneNumber ?? user.PhoneNumber;
            user.Address = profileDto.Address ?? user.Address;

            // Update profile info
            if (user.MemberProfile != null)
            {
                user.MemberProfile.DateOfBirth = profileDto.DateOfBirth ?? user.MemberProfile.DateOfBirth;
                user.MemberProfile.Gender = profileDto.Gender ?? user.MemberProfile.Gender;
                user.MemberProfile.Occupation = profileDto.Occupation ?? user.MemberProfile.Occupation;
                user.MemberProfile.MaritalStatus = profileDto.MaritalStatus ?? user.MemberProfile.MaritalStatus;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

        // DELETE: api/members/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Member not found" });

            // Soft delete (deactivate instead of hard delete)
            user.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Member deactivated successfully" });
        }
    }
}