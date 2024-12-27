using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamFPT.Models.API;
using TeamFPT.Models;
using TeamFPT.Services;

namespace TeamFPT.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly ConnectService _connectService;
		private readonly EmailService _emailService;
		private readonly JwtService _jwtService;
		private readonly IConfiguration _config;

		public UserController(ConnectService connectService, JwtService jwtService, EmailService emailService, IConfiguration configuration)
		{
			_connectService = connectService;
			_jwtService = jwtService;
			_emailService = emailService;
			_config = configuration;
		}
		[Authorize(Roles  = "admin")] 
		[HttpGet("admin")]
		public IActionResult GetAllUsers()
		{
			if (!User.IsInRole("admin"))
			{
				return Forbid("You are not authorized to view this.");
			}

			var allUsers = _connectService.GetAllUsers(); 
			return Ok(allUsers);
		}


		[Authorize]
		[HttpGet("Profile")]
		public IActionResult Profile()
		{
			var isValidClaim = User.Claims.FirstOrDefault(c => c.Type == "isValid");
			if (isValidClaim == null || !bool.TryParse(isValidClaim.Value, out bool isValid) || !isValid)
			{
				return Forbid("You are not Verified.");
			}

			var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var address = User.Claims.FirstOrDefault(c => c.Type == "address")?.Value;
			var phone = User.Claims.FirstOrDefault(c => c.Type == "phone")?.Value;

			var userProfile = new
			{
				Username = username,
				Email = email,
				Address = address,
				Phone = phone
			};

			return Ok(userProfile);
		}



		[AllowAnonymous]
		[HttpPost("Register")]
		public IActionResult Register([FromBody] RegisterRequestModel requestModel)
		{
			List<string> users = _connectService.GetAllUserNames();
			List<string> emails = _connectService.CHeckEmail();
				
			if (users.Contains(requestModel.Username) || emails.Contains(requestModel.Email))
			{
				return BadRequest("username or email existed");
			}

			string otp = _emailService.SendOtpEmail(requestModel.Email);
			_connectService.RegisterUser(requestModel,otp);
			return Ok("sucess");
		}

		[AllowAnonymous]
		[HttpPost("VerifyOtp")]
		public IActionResult VerifyOtp([FromBody] VerifyOtpRequestModel verifyModel)
		{
			OTP oTPDto = _connectService.GetOTP(verifyModel.Email);
				
			if (oTPDto.Value != verifyModel.Otp|| DateTime.UtcNow > oTPDto.Date.AddMinutes(15))
			{
					return BadRequest("Invalid OTP.");
			}

			_connectService.VerifyUser(verifyModel.Email);
			return Ok("Verify Sucess");
			
		}

		[AllowAnonymous]
		[HttpPost("ResetPassword")]
		public IActionResult ResetPass([FromBody] ResetPassRequestModel requestModel)
		{
			if (!_connectService.IsEmailExisted(requestModel))
			{
					return BadRequest("Email Not Existed");
			}

			string otp =_emailService.SendOtpEmail(requestModel.Email);
			_connectService.ResetPassRequest(requestModel.Email,otp);
			return Ok("sucess");
		}

		[AllowAnonymous]
		[HttpPost("ConfirmResetPass")]
		public IActionResult VerifyOtpResetPass([FromBody] VerifyResetPassRequestModel verifyModel)
		{

			OTP oTPDto = _connectService.GetOTP(verifyModel.Email);

			if (verifyModel.Password != verifyModel.RepeatPassword)
			{
				return BadRequest("Password and ConfirmPassword are not match");
			}

			if (oTPDto.Value != verifyModel.Otp || DateTime.UtcNow > oTPDto.Date.AddMinutes(15))
			{
				return BadRequest("Invalid OTP.");
			}

			_connectService.ResetPassword(verifyModel.Email,verifyModel.Password);
			return Ok("Reset Sucess");

		}
		
	}
}
