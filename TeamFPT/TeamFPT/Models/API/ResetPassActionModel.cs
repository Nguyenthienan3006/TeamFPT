namespace TeamFPT.Models.API
{
	public class VerifyResetPassRequestModel
	{
		public string Email { get; set; }
		public string Otp { get; set; }
		public string? Password { get; set; }
		public string? RepeatPassword { get; set; }
	}
}
