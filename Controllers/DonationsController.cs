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
    public class DonationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DonationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/donations/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyDonations()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var donations = await _context.Donations
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return Ok(donations);
        }

        // GET: api/donations/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GetAllDonations()
        {
            var donations = await _context.Donations
                .Include(d => d.User)
                .OrderByDescending(d => d.DonationDate)
                .Select(d => new
                {
                    d.Id,
                    d.Amount,
                    d.Currency,
                    d.Purpose,
                    d.DonationDate,
                    d.PaymentMethod,
                    d.Reference,
                    DonorName = d.User.FullName,
                    DonorEmail = d.User.Email
                })
                .ToListAsync();

            return Ok(donations);
        }

        // GET: api/donations/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> GetDonationStatistics()
        {
            var currentYear = DateTime.UtcNow.Year;
            var currentMonth = DateTime.UtcNow.Month;

            var totalDonations = await _context.Donations.SumAsync(d => d.Amount);
            var monthlyDonations = await _context.Donations
                .Where(d => d.DonationDate.Year == currentYear && d.DonationDate.Month == currentMonth)
                .SumAsync(d => d.Amount);
            var donorCount = await _context.Donations.Select(d => d.UserId).Distinct().CountAsync();

            var monthlyBreakdown = await _context.Donations
                .Where(d => d.DonationDate.Year == currentYear)
                .GroupBy(d => d.DonationDate.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(d => d.Amount) })
                .OrderBy(g => g.Month)
                .ToListAsync();

            return Ok(new
            {
                TotalDonations = totalDonations,
                MonthlyDonations = monthlyDonations,
                DonorCount = donorCount,
                MonthlyBreakdown = monthlyBreakdown
            });
        }

        // POST: api/donations/initiate
        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> InitiateDonation([FromBody] InitiateDonationDto donationDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // For demo purposes - integrate with PayFast, PayPal, or other payment gateway
            var donation = new Donation
            {
                UserId = userId,
                Amount = donationDto.Amount,
                Currency = donationDto.Currency,
                Purpose = donationDto.Purpose,
                PaymentMethod = donationDto.PaymentMethod,
                Status = "Pending",
                DonationDate = DateTime.UtcNow,
                Reference = GenerateReference()
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Return payment URL (integrate with your payment gateway)
            var paymentUrl = $"https://your-payment-gateway.com/pay?reference={donation.Reference}&amount={donation.Amount}";

            return Ok(new { donationId = donation.Id, paymentUrl, reference = donation.Reference });
        }

        // POST: api/donations/webhook (called by payment gateway)
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentWebhook([FromBody] PaymentWebhookDto webhookDto)
        {
            var donation = await _context.Donations
                .FirstOrDefaultAsync(d => d.Reference == webhookDto.Reference);

            if (donation == null)
                return NotFound(new { message = "Donation not found" });

            donation.Status = webhookDto.Status;
            donation.TransactionId = webhookDto.TransactionId;
            donation.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Webhook processed" });
        }

        private string GenerateReference()
        {
            return $"JCAM-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
        }
    }

    public class InitiateDonationDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZAR";
        public string Purpose { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Card";
    }

    public class PaymentWebhookDto
    {
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }
}