using Microsoft.Extensions.Configuration;
using sales.infra.Interfaces;
using System.Net;
using System.Net.Mail;

namespace sales.infra.Services
{
    public class EmailService : IEmailService
    {

        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This is for testing only
        //public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        //{
        //    // STUB IMPLEMENTATION - Console logging for now
        //    // TODO: Integrate with actual email provider (SMTP, SendGrid, etc.)

        //    await Task.Run(() =>
        //    {
        //        Console.WriteLine("===========================================");
        //        Console.WriteLine("📧 EMAIL SENDING (STUB)");
        //        Console.WriteLine("===========================================");
        //        Console.WriteLine($"To: {toEmail}");
        //        Console.WriteLine($"Subject: Your OTP Code");
        //        Console.WriteLine($"Body: Your OTP code is: {otpCode}");
        //        Console.WriteLine($"This code will expire in 5 minutes.");
        //        Console.WriteLine("===========================================");
        //    });

        //    // In production, I will replace this with actual email sending:
        //    // Example with SMTP:
        //    // using var smtpClient = new SmtpClient("smtp.gmail.com", 587);
        //    // smtpClient.Credentials = new NetworkCredential("customer-email@gmail.com", "customer-password");
        //    // smtpClient.EnableSsl = true;
        //    // var mailMessage = new MailMessage("from@example.com", toEmail, "Your OTP Code", $"Your OTP is: {otpCode}");
        //    // await smtpClient.SendMailAsync(mailMessage);
        //}

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"];
            var fromName = smtpSettings["FromName"];

            try
            {
                using var smtpClient = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = "Your OTP Code - Sales Application",
                    Body = $@"
                        <html>
                        <body>
                            <h2>Your OTP Code</h2>
                            <p>Your one-time password is:</p>
                            <h1 style='color: #4CAF50; font-size: 32px;'>{otpCode}</h1>
                            <p>This code will expire in <strong>5 minutes</strong>.</p>
                            <p>If you did not request this code, please ignore this email.</p>
                            <br>
                            <p>Thank you,<br>Sales Application Team</p>
                        </body>
                        </html>
                    ",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);

                Console.WriteLine($"✅ Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                // Log error but don't throw bcoz OTP is still saved in database
                Console.WriteLine($"❌ Email sending failed: {ex.Message}");
            }
        }
    }

}
