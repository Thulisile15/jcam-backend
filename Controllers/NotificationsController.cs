using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using JCAM_CONNECT.Data;
using JCAM_CONNECT.Models;

namespace JCAM_CONNECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public NotificationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/notifications/email/prayer-response
        [HttpPost("email/prayer-response")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> SendPrayerResponseEmail([FromBody] PrayerResponseEmailDto emailDto)
        {
            var prayerRequest = await _context.PrayerRequests
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == emailDto.PrayerRequestId);

            if (prayerRequest == null)
                return NotFound(new { message = "Prayer request not found" });

            if (prayerRequest.IsAnonymous)
                return BadRequest(new { message = "Cannot send email to anonymous request" });

            try
            {
                await SendEmail(
                    prayerRequest.User.Email,
                    "Your Prayer Request Has Been Answered",
                    $@"
                    <h2>Prayer Request Response</h2>
                    <p>Dear {prayerRequest.User.FullName},</p>
                    <p>Your prayer request has been answered by our pastoral team.</p>
                    <p><strong>Your Prayer Request:</strong><br/>{prayerRequest.PrayerRequestText}</p>
                    <p><strong>Response:</strong><br/>{emailDto.ResponseMessage}</p>
                    <p>We continue to pray for you and your family.</p>
                    <p>God bless,<br/>JCAM Connect Church</p>
                    "
                );

                return Ok(new { message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to send email: {ex.Message}" });
            }
        }

        // POST: api/notifications/email/event-reminder
        [HttpPost("email/event-reminder")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> SendEventReminder([FromBody] EventReminderDto reminderDto)
        {
            var eventItem = await _context.Events.FindAsync(reminderDto.EventId);
            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            var members = await _context.Users
                .Where(u => u.Role == "Member" && u.IsActive && u.Email != null)
                .ToListAsync();

            int sentCount = 0;
            foreach (var member in members)
            {
                try
                {
                    await SendEmail(
                        member.Email,
                        $"Reminder: {eventItem.Title}",
                        $@"
                        <h2>Event Reminder</h2>
                        <p>Dear {member.FullName},</p>
                        <p>This is a reminder for the upcoming event:</p>
                        <p><strong>{eventItem.Title}</strong><br/>
                        <strong>Date:</strong> {eventItem.EventDate:dddd, MMMM d, yyyy}<br/>
                        <strong>Time:</strong> {eventItem.StartTime:hh:mm tt}<br/>
                        <strong>Location:</strong> {eventItem.Location}</p>
                        <p>{eventItem.Description}</p>
                        <p>We look forward to seeing you!</p>
                        <p>God bless,<br/>JCAM Connect Church</p>
                        "
                    );
                    sentCount++;
                }
                catch { /* Skip failed emails */ }
            }

            return Ok(new { message = $"Reminder sent to {sentCount} members" });
        }

        // POST: api/notifications/sms/prayer-request (requires SMS service)
        [HttpPost("sms/prayer-request")]
        [Authorize(Roles = "Admin,ChurchLeader")]
        public async Task<IActionResult> SendPrayerResponseSms([FromBody] SmsNotificationDto smsDto)
        {
            // Note: You'll need an SMS provider like Twilio, Africa's Talking, or Clickatell
            // This is a template - integrate with your SMS provider

            var prayerRequest = await _context.PrayerRequests
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == smsDto.PrayerRequestId);

            if (prayerRequest == null)
                return NotFound(new { message = "Prayer request not found" });

            if (string.IsNullOrEmpty(prayerRequest.User.PhoneNumber))
                return BadRequest(new { message = "Member has no phone number" });

            // Example using Africa's Talking (South Africa)
            // var response = await SendSmsAfricaTalking(
            //     prayerRequest.User.PhoneNumber,
            //     $"JCAM Connect: Your prayer request has been answered. {smsDto.ResponseMessage}"
            // );

            return Ok(new { message = "SMS sent successfully (SMS service integration required)" });
        }

        private async Task SendEmail(string to, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");
            using var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]));
            client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
            client.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings["FromEmail"], "JCAM Connect Church"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }
    }

    public class PrayerResponseEmailDto
    {
        public int PrayerRequestId { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
    }

    public class EventReminderDto
    {
        public int EventId { get; set; }
    }

    public class SmsNotificationDto
    {
        public int PrayerRequestId { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
    }
}