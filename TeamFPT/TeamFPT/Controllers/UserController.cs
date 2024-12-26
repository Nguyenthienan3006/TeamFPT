using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamFPT.Models.API;
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

		[AllowAnonymous]
		[HttpPost("Register")]
		public IActionResult Register([FromBody] RegisterRequestModel requestModel)
		{
				_connectService.RegisterUser(requestModel);
				var token = _jwtService.GenerateOtpTken(requestModel);
				_jwtService.SaveOtpToken(token);
				return Ok(token);
		}

		[AllowAnonymous]
		[HttpPost("VerifyOtp")]
		public IActionResult VerifyOtp([FromBody] VerifyOtpRequestModel verifyModel)
		{
				var token = _jwtService.GetStoredOtpToken();
				var claimsPrincipal = ValidateTokenAndExtractClaims(token);
				if (claimsPrincipal == null)
				{
				return BadRequest("Invalid token.");
				}
				var tokenOtp = claimsPrincipal?.FindFirst("otp")?.Value;
				var username = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value; 
				var password = claimsPrincipal.FindFirst("password")?.Value;

			if (tokenOtp != verifyModel.Otp)
				{
					return BadRequest("Invalid OTP.");
				}
			_connectService.VerifyUser(username, password);

			return Ok("Verify Sucess");
			
		}

		private ClaimsPrincipal ValidateTokenAndExtractClaims(string token)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidIssuer = _config["Jwt:Issuer"],
				ValidAudience = _config["Jwt:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
			};
				var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
				return principal;
			
		}
	}
}
