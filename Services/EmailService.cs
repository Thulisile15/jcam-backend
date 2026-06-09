using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace JCAM_CONNECT.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Try Resend API first (works on Render free tier)
                var resendApiKey = _configuration["Resend:ApiKey"];

                if (!string.IsNullOrEmpty(resendApiKey))
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {resendApiKey}");

                    var emailData = new
                    {
                        from = "JCAM Ministries <hello@jcaministries.co.za>",
                        to = new[] { toEmail },
                        subject = subject,
                        html = body
                    };

                    var json = JsonSerializer.Serialize(emailData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://api.resend.com/emails", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Email sent successfully via Resend to {toEmail}");
                        return;
                    }

                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resend failed: {error}");
                }

                // Fallback to Gmail SMTP (if Resend fails or not configured)
                var emailSettings = _configuration.GetSection("EmailSettings");
                var host = emailSettings["Host"];
                var portStr = emailSettings["Port"];
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];

                if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    int port = int.Parse(portStr ?? "587");

                    using var smtpClient = new SmtpClient(host, port);
                    smtpClient.Credentials = new NetworkCredential(username, password);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail ?? username, fromName ?? "JCAM Ministries"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await smtpClient.SendMailAsync(mailMessage);
                    Console.WriteLine($"Email sent successfully via Gmail to {toEmail}");
                    return;
                }

                Console.WriteLine("No email service configured. Skipping email send.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        // ========== ALL YOUR TEMPLATE METHODS BELOW - COMPLETELY UNCHANGED ==========

        public string GetTestimonyReceivedTemplate(string name)
        {
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>JCAM Ministries</h2>");
            html.AppendLine("<p style='color: #f5a623; margin: 5px 0 0;'>Jesus Christ Is The Answer</p>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Dear {name},</h3>");
            html.AppendLine("<p>Thank you for sharing your testimony with us. We have received it and our admin team will review it shortly.</p>");
            html.AppendLine("<p>You will receive another email once your testimony has been approved and published.</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine("<p style='margin: 0; color: #1a3c8f;'><strong>Psalm 66:16</strong><br/>Come and hear, all you who fear God; let me tell you what he has done for me.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p>God bless you,<br/><strong>JCAM Ministries </strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>");
            html.AppendLine("<p>JCAM - Jesus Christ Is The Answer Ministries</p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string GetTestimonyApprovedTemplate(string name, string title)
        {
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>Great News!</h2>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Dear {name},</h3>");
            html.AppendLine($"<p>Your testimony titled <strong>\"{title}\"</strong> has been approved and is now live on our website!</p>");
            html.AppendLine("<p>Thank you for sharing what God has done in your life. Your story will inspire and encourage others.</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine("<p style='margin: 0; color: #1a3c8f;'><strong>Revelation 12:11</strong><br/>They triumphed over him by the blood of the Lamb and by the word of their testimony.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p>God bless you abundantly,<br/><strong>JCAM Ministries </strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string GetPrayerReceivedTemplate(string name, bool isAnonymous)
        {
            var displayName = isAnonymous ? "Friend" : name;
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>Prayer Request Received</h2>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Dear {displayName},</h3>");
            html.AppendLine("<p>We have received your prayer request. Our admin team will review your prayer request immediately.</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine("<p style='margin: 0; color: #1a3c8f;'><strong>Philippians 4:6-7</strong><br/>Do not be anxious about anything, but in every situation, by prayer and petition, with thanksgiving, present your requests to God.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p>We are standing with you in prayer!<br/><strong>JCAM Ministries </strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string GetPrayerAnsweredTemplate(string name, bool isAnonymous)
        {
            var displayName = isAnonymous ? "Friend" : name;
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>Prayer Update</h2>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Dear {displayName},</h3>");
            html.AppendLine("<p>We want you to know that your prayer request has been recieved. Our admin team will get in touch with you.</p>");
            html.AppendLine("<p>We believe that God hears our prayers and is working on your behalf. Stay encouraged!</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine("<p style='margin: 0; color: #1a3c8f;'><strong>Jeremiah 29:12</strong><br/>Then you will call on me and come and pray to me, and I will listen to you.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p>Continuing to pray for you,<br/><strong>JCAM Ministries </strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string GetBaptismReceivedTemplate(string name)
        {
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>Baptism Request Received</h2>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Dear {name},</h3>");
            html.AppendLine("<p>Thank you for your interest in being baptized at JCAM! We have received your request and will be in touch soon.</p>");
            html.AppendLine("<p>Our admin team will review your request and contact you with details about the upcoming baptism service.</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine("<p style='margin: 0; color: #1a3c8f;'><strong>Mark 16:16</strong><br/>Whoever believes and is baptized will be saved.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p>God bless you on this beautiful journey!<br/><strong>JCAM Ministries Team</strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string GetBaptismApprovedTemplate(string name, DateTime baptismDate, string location)
        {
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>Baptism Approved!</h2>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Congratulations, {name}!</h3>");
            html.AppendLine("<p>Your baptism request has been approved. Here are the details:</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine($"<p><strong>Date:</strong> {baptismDate:dddd, MMMM d, yyyy}</p>");
            html.AppendLine($"<p><strong>Location:</strong> {location}</p>");
            html.AppendLine("<p><strong>Time:</strong> Please arrive at 9:00 AM</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p>We are so excited to celebrate this special moment with you!</p>");
            html.AppendLine("<p>With love,<br/><strong>JCAM Ministries</strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string GetCounsellingReceivedTemplate(string name)
        {
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>Counselling Request Received</h2>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Dear {name},</h3>");
            html.AppendLine("<p>We have received your counselling request. Our admin team will review it and contact you within 24 hours.</p>");
            html.AppendLine("<p>You are not alone — we are here to support you.</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine("<p style='margin: 0; color: #1a3c8f;'><strong>Psalm 34:17-18</strong><br/>The righteous cry out, and the Lord hears them; he delivers them from all their troubles. The Lord is close to the brokenhearted and saves those who are crushed in spirit.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p><strong>JCAM Ministries</strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }

        public string GetCounsellingApprovedTemplate(string name, DateTime scheduledDate)
        {
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #f5f7fa;'>");
            html.AppendLine("<div style='background: linear-gradient(135deg, #1a3c8f 0%, #2c5eb8 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
            html.AppendLine("<h2 style='color: white; margin: 0;'>Counselling Session Confirmed</h2>");
            html.AppendLine("</div>");
            html.AppendLine("<div style='background: white; padding: 30px; border-radius: 0 0 10px 10px;'>");
            html.AppendLine($"<h3 style='color: #1a3c8f;'>Dear {name},</h3>");
            html.AppendLine("<p>Your counselling session has been scheduled:</p>");
            html.AppendLine("<div style='background: #f0f4ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
            html.AppendLine($"<p><strong>Date &amp; Time:</strong> {scheduledDate:dddd, MMMM d, yyyy 'at' h:mm tt}</p>");
            html.AppendLine("</div>");
            html.AppendLine("<p>A member of our admin team will be in touch with you.</p>");
            html.AppendLine("<p>God bless you,<br/><strong>JCAM Ministries </strong></p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return html.ToString();
        }
    }
}