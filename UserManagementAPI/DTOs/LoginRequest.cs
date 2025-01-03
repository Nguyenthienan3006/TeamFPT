using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is not blank")]
        public string Password { get; set; } = string.Empty;
    }
}
