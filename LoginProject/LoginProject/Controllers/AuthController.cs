using BCrypt.Net;
using LoginProject.Data;
using LoginProject.DTO;
using LoginProject.Models;
using LoginProject.Repositories;
using LoginProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace LoginProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthController(UsersService usersService, IConfiguration configuration, EmailService emailService)
        {
            _usersService = usersService;
            _config = configuration;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTO.RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (_usersService.GetUserByUsernameAndEmail(request.Username, request.Email) != null) return BadRequest("Username or Email is already existed!");

            var token = Guid.NewGuid().ToString();
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
            };

            if (!_usersService.Register(newUser, token)) return StatusCode(500, "Error occurred during registration");

            await _emailService.SendEmailAsync(newUser.Email, "Verify Your Email",
                   $"Token: {token}");

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

            await _emailService.SendEmailAsync(request.Email, "Verify Your Email", $"Token: {token}");
            return Ok("Verification email resent.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var resetToken = Guid.NewGuid().ToString();
            if (!_usersService.InsertResetPasswordToken(request.Email, resetToken)) return NotFound("Email not found.");

            await _emailService.SendEmailAsync(request.Email, "Password Reset Request", $"Token: {resetToken}");
            return Ok("Password reset email sent.");
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] DTO.ResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!_usersService.ResetPassword(request.Token, BCrypt.Net.BCrypt.HashPassword(request.NewPassword))) return BadRequest("Invalid or expired token.");

            return Ok("Password has been successfully reset.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] DTO.LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = AuthenticateUser(request.Username, request.Password);

            if (user == null) return Unauthorized("Invalid credentials.");
            if (!user.IsEmailVerified) return Unauthorized("Account is not verified.");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private User? AuthenticateUser(string username, string password)
        {
            var user = _usersService.GetUserByUsername(username);

            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

            return user;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
                Issuer = _config["Jwt:Issuer"],
                Expires = DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:TokenValidityMins")),
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
