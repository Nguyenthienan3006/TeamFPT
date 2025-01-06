using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class LoginRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }
    }
}
