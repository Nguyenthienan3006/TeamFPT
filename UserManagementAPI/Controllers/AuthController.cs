using UserManagementAPI.Data;
using UserManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Models;
using UserManagementAPI.DTOs;

namespace JwtAuthDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserStore _userStore;
    private readonly IConfiguration _configuration;

    public AuthController(UserStore userStore, IConfiguration configuration)
    {
        _userStore = userStore;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userStore.UserExistsAsync(request.Username))
        {
            return BadRequest("Username already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email,
            Role = "user"
        };

        await _userStore.AddUserAsync(user);

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var existingUser = await _userStore.GetUserByUsernameAsync(request.Username);
            if (existingUser == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(existingUser);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Error in Login: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }


    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
