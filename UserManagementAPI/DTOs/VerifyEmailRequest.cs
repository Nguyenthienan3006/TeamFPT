namespace UserManagementAPI.DTOs
{
    public class VerifyEmailRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
    }

}
