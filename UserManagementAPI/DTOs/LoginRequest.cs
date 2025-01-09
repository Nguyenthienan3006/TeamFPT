using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is not blank")]
        [MinLength(8, ErrorMessage = "Password must be 8 character")]
        [StringLength(50, ErrorMessage = "Password must not exceed 50 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
