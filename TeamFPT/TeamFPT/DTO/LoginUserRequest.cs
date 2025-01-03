using System.ComponentModel.DataAnnotations;

namespace TeamFPT.DTO
{
    public class LoginUserRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
