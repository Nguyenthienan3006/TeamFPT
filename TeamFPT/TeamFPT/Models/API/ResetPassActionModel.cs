using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
namespace TeamFPT.Models.API
{
	public class VerifyResetPassRequestModel
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "OTP is required.")]
		[StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 characters.")]
		[RegularExpression(@"^[a-zA-Z0-9]{6}$", ErrorMessage = "OTP must be exactly 6 alphanumeric characters with no spaces or special characters.")]
		public string Otp { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
		public string? Password { get; set; }

		[Required(ErrorMessage = "Repeat Password is required.")]
		[Compare("Password", ErrorMessage = "Passwords do not match.")]
		public string? RepeatPassword { get; set; }
	}
}
