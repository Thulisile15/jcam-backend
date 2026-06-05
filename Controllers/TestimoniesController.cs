using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.DTOs;
using JCAM_CONNECT.Models;
using JCAM_CONNECT.Services;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestimoniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailService _emailService;

        public TestimoniesController(ApplicationDbContext context, IWebHostEnvironment environment, EmailService emailService)
        {
            _context = context;
            _environment = environment;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApproved()
        {
            // HIDE ALL TESTIMONIES - Return empty list
            // Uncomment the code below when you want to show testimonies again
            return Ok(new List<object>());

            /* ORIGINAL CODE - COMMENTED OUT
            var testimonies = await _context.Testimonies
                .Where(t => t.IsApproved == true)
                .OrderByDescending(t => t.ApprovedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Content,
                    t.SubmittedAt,
                    t.Email,
                    t.SubmitterName,
                    Documents = t.Documents.Select(d => new { d.FileName, d.FilePath })
                })
                .ToListAsync();

            return Ok(testimonies);
            */
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingTestimonies()
        {
            var pendingTestimonies = await _context.Testimonies
                .Where(t => t.IsApproved == false)
                .OrderByDescending(t => t.SubmittedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Content,
                    t.SubmittedAt,
                    t.Email,
                    t.SubmitterName
                })
                .ToListAsync();

            return Ok(pendingTestimonies);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTestimony([FromForm] TestimonyDto testimonyDto, IFormFileCollection? files)
        {
            var testimony = new Testimony
            {
                Title = testimonyDto.Title,
                Content = testimonyDto.Content,
                Email = testimonyDto.Email,
                SubmitterName = testimonyDto.SubmitterName,
                SubmittedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.Testimonies.Add(testimony);
            await _context.SaveChangesAsync();

            // EMAIL CODE ADDED HERE - Send confirmation email to submitter
            if (!string.IsNullOrEmpty(testimony.Email))
            {
                await _emailService.SendEmailAsync(
                    testimony.Email,
                    "Your Testimony Has Been Received - JCAM Ministries",
                    _emailService.GetTestimonyReceivedTemplate(testimony.SubmitterName ?? "Beloved")
                );
            }

            if (files != null && files.Count > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "testimonies");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var document = new TestimonyDocument
                        {
                            TestimonyId = testimony.Id,
                            FileName = file.FileName,
                            FilePath = $"/uploads/testimonies/{uniqueFileName}",
                            FileType = Path.GetExtension(file.FileName),
                            FileSize = file.Length
                        };

                        _context.TestimonyDocuments.Add(document);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Testimony submitted successfully and pending approval", testimonyId = testimony.Id });
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveTestimony(int id)
        {
            var testimony = await _context.Testimonies.FindAsync(id);
            if (testimony == null)
                return NotFound(new { message = "Testimony not found" });

            testimony.IsApproved = true;
            testimony.ApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // EMAIL CODE ADDED HERE - Send approval email to submitter
            if (!string.IsNullOrEmpty(testimony.Email))
            {
                await _emailService.SendEmailAsync(
                    testimony.Email,
                    "Your Testimony Has Been Published - JCAM Ministries",
                    _emailService.GetTestimonyApprovedTemplate(testimony.SubmitterName ?? "Beloved", testimony.Title ?? "")
                );
            }

            return Ok(new { message = "Testimony approved successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestimony(int id)
        {
            var testimony = await _context.Testimonies
                .Include(t => t.Documents)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (testimony == null)
                return NotFound(new { message = "Testimony not found" });

            foreach (var doc in testimony.Documents)
            {
                var physicalPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }

            _context.Testimonies.Remove(testimony);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Testimony deleted successfully" });
        }
    }
}