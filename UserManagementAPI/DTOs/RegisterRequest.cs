using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Passworg is not blank")]
        [MinLength(8, ErrorMessage = "Password must be 8 character")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "email is not blank")]
        [EmailAddress(ErrorMessage = "Email wrong")]
        public string Email { get; set; } = string.Empty;
    }
}
