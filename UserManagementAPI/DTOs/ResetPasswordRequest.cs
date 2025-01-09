using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        [MinLength(8, ErrorMessage = "Username must be 8 character")]
        public string Username { get; set; }
        public string Otp { get; set; }
        [Required(ErrorMessage = "Password is not blank")]
        [MinLength(8, ErrorMessage = "Password must be 8 character")]
        [StringLength(50, ErrorMessage = "Password must not exceed 50 characters")]
        public string NewPassword { get; set; }
    }
}
