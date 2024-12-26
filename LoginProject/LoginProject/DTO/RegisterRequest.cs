using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
       // [MinLength(8)]
        public string Password { get; set; }
    }
}
