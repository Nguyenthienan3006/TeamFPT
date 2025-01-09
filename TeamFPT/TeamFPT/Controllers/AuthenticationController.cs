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
		private readonly RedisService _redisService;


		public AuthenticationController(ConnectService connectService, JwtService jwtService , RedisService redis)
		{
			_connectService = connectService ;
			_jwtService = jwtService;
			_redisService = redis;
		}

		[AllowAnonymous]
		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequestModel userLogin)
		{
			if (userLogin == null || string.IsNullOrEmpty(userLogin.UserName) || string.IsNullOrEmpty(userLogin.PassWord))	return BadRequest("Invalid login details.");
			var user = _connectService.Authenticate(userLogin);
			if (user == null) return BadRequest("Worng username or password");
			var token = _jwtService.GenerateToken(user);
			_redisService.SaveTokenToRedisAsync(token, user.Id);
			return Ok(token);
		}
	}

}
