using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
