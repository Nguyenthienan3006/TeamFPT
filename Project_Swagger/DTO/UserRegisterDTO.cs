using System;
using System.ComponentModel.DataAnnotations;

namespace Project_Swagger.DTO
{
    public class UserRegisterDTO
    {
        [Required(ErrorMessage = "Must enter username")]
        [StringLength(50, ErrorMessage = "Invalid username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Must enter fullname")]
        [StringLength(255, ErrorMessage = "Name too long")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Must enter password")]
        [StringLength(255, ErrorMessage = "Invalid password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Must enter email")]
        [StringLength(50, ErrorMessage = "Invalid email")]
        public string Email { get; set; }
        public string Role { get; } = "user";
        public string OTP { get; } = GenerateRandomOtp();
        public string TypeCode { get; } = "verify";
        private static string GenerateRandomOtp()
        {
            Random random = new Random();
            int otp = random.Next(100000, 999999); 
            return otp.ToString(); 
        }
    }
}
