namespace LoginProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpiration { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiration { get; set; }
    }

}
