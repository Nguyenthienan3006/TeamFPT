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
            // Implement email sending logic here using SMTP or your preferred email service
            // Example template:
            string template = $@"
            <h2>Email Verification</h2>
            <p>Your OTP code is: <strong>{otp}</strong></p>
            <p>This code will expire in 2 minutes.</p>
            <p>If you didn't request this code, please ignore this email.</p>";

            // Send email implementation
        }
    }
}
