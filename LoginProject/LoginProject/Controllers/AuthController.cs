using LoginProject.Data;
using LoginProject.DTO;
using LoginProject.Models;
using LoginProject.Repositories;
using LoginProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
            try
            {
                if (IsUsernameOrEmailExist(request.Username, request.Email))
                {
                    return BadRequest("Username or Email is already existed!");
                }

                var token = Guid.NewGuid().ToString();

                var newUser = new User
                {
                    Username = request.Username,
                    Password = request.Password,
                    Email = request.Email,
                    VerificationToken = token,
                    TokenExpiration = DateTime.UtcNow.AddHours(24)
                };
                if (_usersService.Register(newUser))
                {

                    await _emailService.SendEmailAsync(newUser.Email, "Verify Your Email",
                        $"Token: {token}");

                    return Ok("User registered. Please verify your email.");
                }

                return StatusCode(500, "Error occurred during registration");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }


        }
        private bool IsUsernameOrEmailExist(string username, string email)
        {
            return _usersService.GetUserByUsername(username) != null || _usersService.GetUserByEmail(email) != null;
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                var user = _usersService.GetUserByVerificationToken(token);

                if (user == null || user.TokenExpiration < DateTime.UtcNow)
                {
                    return BadRequest("Invalid or expired token.");
                }

                if (_usersService.VerifyEmail(user.Id))
                {
                    return Ok("Email verified successfully.");
                }
                return StatusCode(500, "Error occurred during registration");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail([FromQuery] ResendVerificationRequest request)
        {
            try
            {
                var user = _usersService.GetUserByEmail(request.Email);

                if (user == null || user.IsVerified)
                {
                    return BadRequest("User not found or already verified.");
                }

                var token = Guid.NewGuid().ToString();


                user.VerificationToken = token;
                user.TokenExpiration = DateTime.UtcNow.AddHours(1);

                if (_usersService.UpdateVerificationToken(user))
                {

                    await _emailService.SendEmailAsync(request.Email, "Verify Your Email",
                        $"Token: {token}");

                    return Ok("Verification email resent.");
                }

                return StatusCode(500, "Error occurred during creating verification token.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] DTO.LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                User user = _usersService.AuthenticateUser(request.Username, request.Password);
                if (user == null) return Unauthorized("Invalid credentials.");
                if (!user.IsVerified) return Unauthorized("Account is not verified.");


                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }



        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
