using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Models.API
{
	public class ResetPassRequestModel
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		public string? Email { get; set; }
		public string? Username { get; set; }
	}
}
