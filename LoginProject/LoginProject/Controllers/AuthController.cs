using LoginProject.DTO;
using LoginProject.Models;
using LoginProject.Repositories;
using LoginProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly RedisService _redisService;
        private readonly JwtService _jwtService;

        public AuthController(UsersService usersService, IConfiguration configuration, EmailService emailService, RedisService redisService, JwtService jwtService)
        {
            _usersService = usersService;
            _config = configuration;
            _emailService = emailService;
            _redisService = redisService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTO.RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (_usersService.GetUserByUsername(request.Username) != null) return BadRequest("Username is already existed");
            if (_usersService.GetUserByEmail(request.Email) != null) return BadRequest("Email is already existed");

            var token = Guid.NewGuid().ToString();
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = _usersService.HashPassword(request.Password),
                Email = request.Email,
            };

            if (!_usersService.Register(newUser, token)) return StatusCode(500, "Error occurred during registration");

            await _emailService.SendEmailAsync(newUser.Email, "Verify Your Email", _emailService.GenerateVerificationAccountEmail(token));
            return Ok("User registered. Please verify your email.");
        }

        [HttpGet("verify")]
        public IActionResult VerifyEmail([FromQuery] string token)
        {
            if (!_usersService.VerifyEmail(token)) return BadRequest("Invalid or expired token.");

            return Ok("Email verified successfully.");
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var token = Guid.NewGuid().ToString();
            if (!_usersService.InsertVerificationToken(request.Email, token)) return NotFound("User not found or already verified.");

            await _emailService.SendEmailAsync(request.Email, "Verify Your Email", _emailService.GenerateVerificationAccountEmail(token));
            return Ok("Verification email resent.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var resetToken = Guid.NewGuid().ToString();
            if (!_usersService.InsertResetPasswordToken(request.Email, resetToken)) return NotFound("Email not found.");

            await _emailService.SendEmailAsync(request.Email, "Password Reset Request", _emailService.GenerateResetPasswordEmail(resetToken));
            return Ok("Password reset email sent.");
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] DTO.ResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!_usersService.ResetPassword(request.Token, _usersService.HashPassword(request.NewPassword))) return BadRequest("Invalid or expired token.");

            return Ok("Password has been successfully reset.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTO.LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = _usersService.ValidateUser(request.Username, request.Password);
            if (user == null) return Unauthorized("Invalid credentials.");
            if (!user.IsEmailVerified) return Unauthorized("Account is not verified.");

            var token = await _jwtService.GenerateJwtToken(user);        
            return Ok(new { Token = token });
        }

        [HttpGet("get-token")]
        public async Task<IActionResult> GetToken([FromQuery] string key)
        {
            var token = await _redisService.GetCacheAsync(key);
            if (token == null) return NotFound("Token not found or expired.");
            return Ok(new { Token = token });
        }

        [HttpPost("test-send-20-mail")]
        public async Task<IActionResult> Test()
        {
            await _emailService.SendEmailAsync("ducdmhe181735@fpt.edu.vn", "test", "test");
            return Ok("Test completed");
        }


    }
}
