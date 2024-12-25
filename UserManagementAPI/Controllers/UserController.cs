using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("register")]
    public IActionResult Register(string username, string password, string email)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            connection.Open();
            using (var command = new MySqlCommand("CALL AddUser(@username, @passwordHash, @email)", connection))
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                command.Parameters.AddWithValue("@email", email);

                try
                {
                    command.ExecuteNonQuery();
                    return Ok("User registered successfully");
                }
                catch (MySqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }

    [HttpGet("users")]
    [Authorize]
    public IActionResult GetUsers()
    {
        using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            connection.Open();
            using (var command = new MySqlCommand("CALL GetUsers()", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    var users = new List<object>();
                    while (reader.Read())
                    {
                        users.Add(new
                        {
                            Id = reader["id"],
                            Username = reader["username"],
                            Email = reader["email"],
                            CreatedAt = reader["created_at"]
                        });
                    }
                    return Ok(users);
                }
            }
        }
    }
}
