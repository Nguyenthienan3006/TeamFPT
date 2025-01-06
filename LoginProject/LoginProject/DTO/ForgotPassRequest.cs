using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class ForgotPassRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }


    }
}
