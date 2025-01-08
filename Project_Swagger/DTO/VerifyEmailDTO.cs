using System.ComponentModel.DataAnnotations;

namespace Project_Swagger.DTO
{
    public class VerifyEmailDTO
    {
        [Required(ErrorMessage = "Must enter email")]
        [StringLength(50, ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Must enter OTP")]
        [StringLength(7, ErrorMessage = "Invalid OTP")]
        public string OTP { get; set; }
    }
}
