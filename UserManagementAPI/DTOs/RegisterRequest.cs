using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is not blank")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Passworg is not blank")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "email is not blank")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
