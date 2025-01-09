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
		private readonly RedisService _redisService;

		public UserController(ConnectService connectService, JwtService jwtService, EmailService emailService, IConfiguration configuration, RedisService redisService)
		{
			_connectService = connectService;
			_jwtService = jwtService;
			_emailService = emailService;
			_config = configuration;
			_redisService = redisService;
		}
		[Authorize(Roles  = "admin")] 
		[HttpGet("admin")]
		public IActionResult GetAllUsers()
		{
			if (!User.IsInRole("admin"))return Forbid("You are not authorized to view this.");

			var allUsers = _connectService.GetAllUsers(); 
			return Ok(allUsers);
		}


		[Authorize]
		[HttpGet("Profile/{id}")]
		public IActionResult Profile(int id)
		{
			if(id==null) return BadRequest("id input is null");
			if (_redisService.GetTokenFromRedisAsync(id) == null) return BadRequest("token is null");
			User user = _connectService.GetUserById(id);
			if(user==null) return BadRequest("user not existed");

			return Ok(user);
		}



		[AllowAnonymous]
		[HttpPost("Register")]
		public IActionResult Register([FromBody] RegisterRequestModel requestModel)
		{
			int result = _connectService.CheckRegister(requestModel.Email,requestModel.Username);
			if (result == 1)return BadRequest("Username or email already exists.");

			string otp = _emailService.GenerateOtp();
			_connectService.RegisterUser(requestModel, otp);
			_emailService.SendOtpEmail(requestModel.Email, otp);
			return Ok("sucess");
		}

		[AllowAnonymous]
		[HttpPost("VerifyOtp")]
		public IActionResult VerifyOtp([FromBody] VerifyOtpRequestModel verifyModel)
		{
			OTP oTPDto = _connectService.GetOTP(verifyModel.Email , "registration");
			if (oTPDto.Value != verifyModel.Otp|| DateTime.UtcNow > oTPDto.Date.AddMinutes(15)) return BadRequest("Invalid OTP.");

			_connectService.VerifyUser(verifyModel.Email);
			return Ok("Verify Sucess");
			
		}

		[AllowAnonymous]
		[HttpPost("ResetPassword")]
		public IActionResult ResetPass([FromBody] ResetPassRequestModel requestModel)
		{
			if (_connectService.CheckRegister(requestModel.Email,requestModel.Username)<1) return BadRequest("Email or Username Not Existed");

			string otp = _emailService.GenerateOtp();
			_connectService.ResetPassRequest(requestModel.Email,otp);
			_emailService.SendOtpEmail(requestModel.Email, otp);

			return Ok("sucess");
		}

		[AllowAnonymous]
		[HttpPost("ConfirmResetPass")]
		public IActionResult VerifyOtpResetPass([FromBody] VerifyResetPassRequestModel verifyModel)
		{
			OTP oTPDto = _connectService.GetOTP(verifyModel.Email, "resetPassord");
			if (oTPDto.Value != verifyModel.Otp || DateTime.UtcNow > oTPDto.Date.AddMinutes(15)) return BadRequest("Invalid OTP.");

			_connectService.ResetPassword(verifyModel.Email,verifyModel.Password);
			return Ok("Reset Sucess");
		}
		
	}
}
