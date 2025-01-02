using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        public string Username { get; set; }
        public string Otp { get; set; }
        [Required(ErrorMessage = "Password is not blank")]
        public string NewPassword { get; set; }
    }
}
