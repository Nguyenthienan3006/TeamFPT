using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFPT.Models.API;
using TeamFPT.Services;


namespace TeamFPT.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthenticationController : ControllerBase
	{
		private readonly ConnectService _connectService;
		private readonly JwtService _jwtService;

		public AuthenticationController(ConnectService connectService, JwtService jwtService)
		{
			_connectService = connectService ;
			_jwtService = jwtService;
		}

		[AllowAnonymous]
		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequestModel userLogin)
		{
			if (userLogin == null || string.IsNullOrEmpty(userLogin.UserName) || string.IsNullOrEmpty(userLogin.PassWord))
			{
				return BadRequest("Invalid login details.");
			}

			var user = _connectService.Authenticate(userLogin);

			if (user != null && user.IsValid==true)
			{
				var token = _jwtService.GenerateToken(user);

				
				return Ok(token);
			}

			return Unauthorized("Invalid credentials.");
		}
	}

}
