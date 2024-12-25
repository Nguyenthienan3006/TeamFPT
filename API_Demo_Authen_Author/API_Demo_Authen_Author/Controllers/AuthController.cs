using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Demo_Authen_Author.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(DemoAPIContext context, IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto userLogin)
        {
            var user = Authenticate(userLogin);

            if (user != null)
            {
                var token = GenerateToken(user);
                return Ok(token);
            }

            return NotFound();
        }

        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Cách 1
            //var claims = new[]
            //{
            //    new Claim(ClaimTypes.NameIdentifier, user.Username),
            //    new Claim(ClaimTypes.Email, user.Email),
            //    new Claim(ClaimTypes.Name, user.FullName),
            //    new Claim(ClaimTypes.Role, user.Role)
            //};

            //var token = new JwtSecurityToken
            //    (
            //        _config["Jwt:Issuer"],
            //        _config["Jwt:Audience"],
            //        claims,
            //        expires: DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:TokenValidityMins")),
            //        signingCredentials: credentials
            //    );

            //return new JwtSecurityTokenHandler().WriteToken(token);

            //Cách 2 (Dễ nhìn hơn)

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Issuer = _config["Jwt:Issuer"],
                Expires = DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:TokenValidityMins")),
                Audience = _config["Jwt:Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }

        private User Authenticate(LoginDto userLogin)
        {
            User currentUser = null;

            var _connectionString = _config.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("AuthenticateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("p_username", userLogin.UserName);
                    command.Parameters.AddWithValue("p_password", userLogin.PassWord);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentUser = new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                FullName = reader.GetString("fullname"),
                                Role = reader.GetString("role")
                            };
                        }
                    }
                }
            }

            return currentUser;
        }
    }
}
