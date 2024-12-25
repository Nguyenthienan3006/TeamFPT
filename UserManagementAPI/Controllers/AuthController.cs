using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MySql.Data.MySqlClient;
using UserManagementAPI.Models;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] User loginRequest)
    {
        // Kết nối tới MySQL và gọi Stored Procedure
        using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        connection.Open();

        using var cmd = new MySqlCommand("GetUserByUsername", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@p_username", loginRequest.Username);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var storedPassword = reader["PasswordHash"].ToString();

            // Kiểm tra mật khẩu (giả định hash đã được kiểm tra ở đây)
            if (storedPassword != loginRequest.PasswordHash) return Unauthorized();

            // Tạo JWT Token
            var token = GenerateJwtToken(loginRequest.Username);
            return Ok(new { Token = token });
        }

        return Unauthorized();
    }

    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
