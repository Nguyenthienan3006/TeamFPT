using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class ResendVerificationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
