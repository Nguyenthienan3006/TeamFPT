namespace UserManagementAPI.DTOs
{
    public class ResetPasswordRequest
    {
        public string Username { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }
}
