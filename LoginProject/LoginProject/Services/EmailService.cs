using System.Net.Mail;
using System.Net;

namespace LoginProject.Services
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
            var smtpClient = new SmtpClient
            {
                Host = _configuration["EmailSettings:Host"],
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
