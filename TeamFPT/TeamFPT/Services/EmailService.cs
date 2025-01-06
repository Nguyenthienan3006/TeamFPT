using System.Net;
using System.Net.Mail;

namespace TeamFPT.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string email, string otp)
        {
            // Tạo nội dung email
            string template = $@"
            <h2>Email Verification</h2>
            <p>Your OTP code is: <strong>{otp}</strong></p>
            <p>This code will expire in 2 minutes.</p>
            <p>If you didn't request this code, please ignore this email.</p>";

            try
            {
                // Lấy thông tin cấu hình từ appsettings.json
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPass = _configuration["Email:SmtpPass"];
                var fromEmail = _configuration["Email:FromEmail"];

                // Tạo đối tượng MailMessage
                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, "TeamFPT"),
                    Subject = "Email Verification",
                    Body = template,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                // Tạo đối tượng SmtpClient
                using var smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true // Bật SSL nếu máy chủ yêu cầu
                };

                // Gửi email
                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi gửi email
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }
    }
}
