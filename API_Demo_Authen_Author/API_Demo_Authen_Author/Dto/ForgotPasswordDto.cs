using System.ComponentModel.DataAnnotations;

namespace API_Demo_Authen_Author.Dto
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email length cannot exceed 100 characters.")]
        public string Email { get; set; }
    }
}
