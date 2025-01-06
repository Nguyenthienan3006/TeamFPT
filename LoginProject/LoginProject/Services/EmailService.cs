using System.Net;
using System.Net.Mail;
using static System.Net.WebRequestMethods;

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

        public string GenerateVerificationAccountEmail(string token)
        {
            return $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding: 20px;"">
        <tr>
            <td>
                <h1 style=""color: #444;"">Verify Your Account</h1>
                <p>Hello,</p>
                <p>Thank you for creating an account with us. To complete your registration, please verify your email address by clicking the button below:</p>
                
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                    <tr>
                        <td align=""center"" style=""padding: 20px 0;"">
                            <a href=""http://localhost:5000/api/Auth/verify?token={token}"" style=""background-color: #007bff; color: #ffffff; padding: 12px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;"">Verify Your Account</a>
                        </td>
                    </tr>
                </table>
                
                <p>Please note that this verification link will expire in <strong>1 hours</strong>.</p>
                <p>If you did not create an account with us, please disregard this email.</p>
            </td>
        </tr>
    </table>";
        }

        public string GenerateResetPasswordEmail(string token)
        {
            return $@" <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding: 20px;"">
        <tr>
            <td>
                <h1 style=""color: #444;"">Reset Your Password</h1>
                <p>Hello,</p>
                <p>We received a request to reset the password for your account. If you didn't make this request, you can safely ignore this email.</p>
                <p>To reset your password, click the button below:</p>
                
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                    <tr>
                        <td align=""center"" style=""padding: 20px 0;"">
                            <a href=""http://localhost:5000/api/Auth/reset-password?token={token}"" style=""background-color: #28a745; color: #ffffff; padding: 12px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;"">Reset Your Password</a>
                        </td>
                    </tr>
                </table>
                
                <p>This password reset link will expire in <strong>1 hour</strong>.</p>
                <p>If you didn't request a password reset, please contact our support team immediately.</p>
            </td>
        </tr>
    </table>";
        }

        public async Task SendMultipleEmailsAsync(string toEmail, string subject, string body, int numberOfRequests)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < numberOfRequests; i++)
            {
                tasks.Add(SendEmailAsync(toEmail, subject + i, body + i));
            }

            // Chờ cho tất cả các task hoàn thành
            await Task.WhenAll(tasks);
        }

    }
}
