using LoginProject.Data;
using LoginProject.DTO;
using LoginProject.Models;
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
        private readonly DatabaseHelper _dbHelper;
        private readonly IConfiguration _config;

        public AuthController(DatabaseHelper dbHelper, IConfiguration configuration)
        {
            _dbHelper = dbHelper;
            _config = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO request)
        {
            // Xác thực người dùng bằng Stored Procedure
            User user = AuthenticateUser(request.Username, request.Password);
            if (user == null) return Unauthorized("Invalid credentials.");

            // Tạo JWT token
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private User AuthenticateUser(string username, string password)
        {
            using var connection = _dbHelper.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "GetUserByUsername";
            command.CommandType = CommandType.StoredProcedure;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "input";
            parameter.Value = username;
            command.Parameters.Add(parameter);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                if (password == reader["password"].ToString()) // Demo: So sánh mật khẩu trực tiếp
                {
                    return new User
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Role = reader["role"].ToString()
                    };
                }
            }
            return null;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, user.Username),
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
