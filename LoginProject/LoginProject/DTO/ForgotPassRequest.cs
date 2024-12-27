using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class ForgotPassRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }


    }
}
