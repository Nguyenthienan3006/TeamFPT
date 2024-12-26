
using System.Net.Mail;
using System.Net;

namespace API_Demo_Authen_Author.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var smtp = new SmtpClient
                {
                    Host = _configuration["Email:Smtp:Host"],
                    Port = _configuration.GetValue<int>("Email:Smtp:Port"),
                    Credentials = new NetworkCredential(
                        _configuration["Email:Smtp:User"],
                        _configuration["Email:Smtp:Password"]
                    ),
                    EnableSsl = true
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:From"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(to);
                await smtp.SendMailAsync(message);

                // Nếu không xảy ra lỗi, trả về true
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

    }
}
