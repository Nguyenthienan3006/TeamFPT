using System.ComponentModel.DataAnnotations;

namespace Project_Swagger.DTO
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Must enter email")]
        [StringLength(50, ErrorMessage = "Invalid email")]
        public string email { get; set; }

        public string OTP { get; } = GenerateRandomOtp();
        public string TypeCode { get; } = "ChangePassword";
        private static string GenerateRandomOtp()
        {
            Random random = new Random();
            int otp = random.Next(100000, 999999);
            return otp.ToString();
        }
    }
}
