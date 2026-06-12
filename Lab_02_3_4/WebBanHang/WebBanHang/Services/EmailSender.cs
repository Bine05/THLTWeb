using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebBanHang.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _email;
        private readonly string _password;

        public EmailSender(IConfiguration configuration)
        {
            _email = configuration["EmailSettings:Email"] ?? "your-email@gmail.com";
            _password = configuration["EmailSettings:Password"] ?? "your-app-password";
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (_email == "your-email@gmail.com")
            {
                // Fallback nếu chưa cấu hình trong appsettings.json
                Console.WriteLine($"[EmailSender] Chưa cấu hình Email. Link phục hồi: {htmlMessage}");
                return;
            }

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_email, _password)
            };

            var mailMessage = new MailMessage(
                from: _email,
                to: email,
                subject: subject,
                body: htmlMessage
            )
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}
