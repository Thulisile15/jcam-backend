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
    public class DevotionalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DevotionalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/devotionals (all published)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPublished()
        {
            var devotionals = await _context.Devotionals
                .Include(d => d.Author)
                .Where(d => d.IsPublished == true)
                .OrderByDescending(d => d.PublishedAt)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Content,
                    d.BibleVerse,
                    d.PublishedAt,
                    AuthorName = d.Author != null ? d.Author.FullName : "Church Admin"
                })
                .ToListAsync();

            return Ok(devotionals);
        }

        // GET: api/devotionals/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDevotionalById(int id)
        {
            var devotional = await _context.Devotionals
                .Include(d => d.Author)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsPublished == true);

            if (devotional == null)
                return NotFound(new { message = "Devotional not found" });

            return Ok(devotional);
        }

        // GET: api/devotionals/all (admin view all including drafts)
        [HttpGet("all")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GetAllDevotionals()
        {
            var devotionals = await _context.Devotionals
                .Include(d => d.Author)
                .OrderByDescending(d => d.PublishedAt)
                .ToListAsync();

            return Ok(devotionals);
        }

        // POST: api/devotionals
        [HttpPost]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> CreateDevotional([FromBody] CreateDevotionalDto devotionalDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var devotional = new Devotional
            {
                Title = devotionalDto.Title,
                Content = devotionalDto.Content,
                BibleVerse = devotionalDto.BibleVerse,
                AuthorId = userId,
                PublishedAt = devotionalDto.IsPublished ? DateTime.UtcNow : DateTime.UtcNow,
                IsPublished = devotionalDto.IsPublished
            };

            _context.Devotionals.Add(devotional);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Devotional created successfully", devotionalId = devotional.Id });
        }

        // PUT: api/devotionals/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> UpdateDevotional(int id, [FromBody] UpdateDevotionalDto devotionalDto)
        {
            var devotional = await _context.Devotionals.FindAsync(id);
            if (devotional == null)
                return NotFound(new { message = "Devotional not found" });

            devotional.Title = devotionalDto.Title ?? devotional.Title;
            devotional.Content = devotionalDto.Content ?? devotional.Content;
            devotional.BibleVerse = devotionalDto.BibleVerse ?? devotional.BibleVerse;

            if (devotionalDto.IsPublished.HasValue && devotionalDto.IsPublished.Value && !devotional.IsPublished)
            {
                devotional.IsPublished = true;
                devotional.PublishedAt = DateTime.UtcNow;
            }
            else if (devotionalDto.IsPublished.HasValue)
            {
                devotional.IsPublished = devotionalDto.IsPublished.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Devotional updated successfully" });
        }

        // DELETE: api/devotionals/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDevotional(int id)
        {
            var devotional = await _context.Devotionals.FindAsync(id);
            if (devotional == null)
                return NotFound(new { message = "Devotional not found" });

            _context.Devotionals.Remove(devotional);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Devotional deleted successfully" });
        }
    }

    public class CreateDevotionalDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? BibleVerse { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    public class UpdateDevotionalDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? BibleVerse { get; set; }
        public bool? IsPublished { get; set; }
    }
}