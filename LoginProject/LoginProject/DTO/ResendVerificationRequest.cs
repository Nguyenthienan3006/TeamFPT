using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class ResendVerificationRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
    }
}
