using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
