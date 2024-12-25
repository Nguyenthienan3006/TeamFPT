using LoginProject.Data;
using LoginProject.DTO;
using LoginProject.Models;
using LoginProject.Repositories;
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
        private readonly UsersRepository _usersRepository;
        private readonly IConfiguration _config;

        public AuthController(UsersRepository usersRepository, IConfiguration configuration)
        {
            _usersRepository = usersRepository;
            _config = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User request)
        {
            try
            {
                if (IsUsernameOrEmailExist(request.Username, request.Email))
                {
                    return BadRequest("Username or Email is already existed!");
                }

                var newUser = new User
                {
                    Username = request.Username,
                    Password = request.Password,
                    Email = request.Email,
                };
                if (_usersRepository.Register(newUser))
                {
                    return Ok("Register successfully");
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
            return _usersRepository.GetUserByUsername(username) != null || _usersRepository.GetUserByEmail(email) != null;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                User user = _usersRepository.AuthenticateUser(request.Username, request.Password);
                if (user == null) return Unauthorized("Invalid credentials.");


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
