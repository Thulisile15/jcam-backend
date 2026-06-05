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
    public class PrayerRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public PrayerRequestsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPrayerRequests()
        {
            var pendingRequests = await _context.PrayerRequests
                .Where(p => p.IsPrayedFor == false)
                .OrderByDescending(p => p.SubmittedAt)
                .Select(p => new
                {
                    p.Id,
                    p.PrayerRequestText,
                    p.SubmittedAt,
                    p.IsAnonymous,
                    p.Email,
                    p.SubmitterName
                })
                .ToListAsync();

            return Ok(pendingRequests);
        }

        // FIXED: Changed from [HttpGet] to [HttpGet("all")] to match frontend
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPrayerRequests()
        {
            var prayerRequests = await _context.PrayerRequests
                .OrderByDescending(p => p.SubmittedAt)
                .Select(p => new
                {
                    p.Id,
                    p.PrayerRequestText,
                    p.SubmittedAt,
                    p.IsAnonymous,
                    p.IsPrayedFor,
                    p.Email,
                    p.SubmitterName
                })
                .ToListAsync();

            return Ok(prayerRequests);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPrayerRequest([FromBody] PrayerRequestDto prayerDto)
        {
            var prayerRequest = new PrayerRequest
            {
                PrayerRequestText = prayerDto.PrayerRequestText,
                IsAnonymous = prayerDto.IsAnonymous,
                Email = prayerDto.Email,
                SubmitterName = prayerDto.SubmitterName,
                SubmittedAt = DateTime.UtcNow,
                IsPrayedFor = false
            };

            _context.PrayerRequests.Add(prayerRequest);
            await _context.SaveChangesAsync();

            // EMAIL CODE ADDED - Send confirmation email to submitter
            if (!string.IsNullOrEmpty(prayerRequest.Email))
            {
                await _emailService.SendEmailAsync(
                    prayerRequest.Email,
                    "Your Prayer Request Has Been Received - JCAM Ministries",
                    _emailService.GetPrayerReceivedTemplate(prayerRequest.SubmitterName ?? "Beloved", prayerRequest.IsAnonymous)
                );
            }

            return Ok(new { message = "Prayer request submitted successfully", requestId = prayerRequest.Id });
        }

        [HttpPut("{id}/mark-prayed")]
        public async Task<IActionResult> MarkAsPrayedFor(int id)
        {
            var prayerRequest = await _context.PrayerRequests.FindAsync(id);
            if (prayerRequest == null)
                return NotFound(new { message = "Prayer request not found" });

            prayerRequest.IsPrayedFor = true;
            prayerRequest.PrayedForAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // EMAIL CODE ADDED - Send notification that someone prayed for them
            if (!string.IsNullOrEmpty(prayerRequest.Email))
            {
                await _emailService.SendEmailAsync(
                    prayerRequest.Email,
                    "Someone Prayed For Your Request - JCAM Ministries",
                    _emailService.GetPrayerAnsweredTemplate(prayerRequest.SubmitterName ?? "Beloved", prayerRequest.IsAnonymous)
                );
            }

            return Ok(new { message = "Prayer request marked as prayed for" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrayerRequest(int id)
        {
            var prayerRequest = await _context.PrayerRequests.FindAsync(id);
            if (prayerRequest == null)
                return NotFound(new { message = "Prayer request not found" });

            _context.PrayerRequests.Remove(prayerRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Prayer request deleted successfully" });
        }
    }
}