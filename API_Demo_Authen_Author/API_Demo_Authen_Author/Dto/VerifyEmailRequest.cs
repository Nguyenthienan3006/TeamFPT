using System.ComponentModel.DataAnnotations;

namespace API_Demo_Authen_Author.Dto
{
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Token is required.")]
        [StringLength(50, ErrorMessage = "Token cannot exceed 50 characters.")]
        public string token { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string email { get; set; }
    }
}
