using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class EmailVerificationRequest
    {
        [EmailAddress(ErrorMessage = "Wrong format email")]
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
