using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Models.API
{
	public class RegisterRequestModel
	{
		[Required(ErrorMessage = "Username is required.")]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
		public string? Username { get; set; }

		[Required(ErrorMessage = "Password is required.")]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Password must be at least 3 characters long.")]
		public string? Password { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		public string? Email { get; set; }

		public string? Address { get; set; }

		[Phone(ErrorMessage = "Invalid phone number.")]
		public string? Phone { get; set; }
	}

}
