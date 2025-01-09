using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Models.API
{
	public class VerifyOtpRequestModel
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "OTP is required.")]
		[StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 characters.")]
		[RegularExpression(@"^[a-zA-Z0-9]{6}$", ErrorMessage = "OTP must be exactly 6 alphanumeric characters with no spaces or special characters.")]
		public string Otp { get; set; }

	}
}
