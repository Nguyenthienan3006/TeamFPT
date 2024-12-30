using System.ComponentModel.DataAnnotations;

namespace API_Demo_Authen_Author.Dto
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string PassWord { get; set; }
    }
}
