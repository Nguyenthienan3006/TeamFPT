using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using API_Demo_Authen_Author.Dto;
using System.Security.Claims;

namespace API_Demo_Authen_Author.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<List<UserDto>> FetchUsers()
        {
            var users = new List<UserDto>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_GetAllUsers", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new UserDto
                {
                    Id = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role")
                });
            }

            return users;
        }

        [HttpGet("Admin")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminGetUsers()
        {
            var users = await FetchUsers();
            return Ok(new 
            { 
                Name = ClaimTypes.Name,
                Role = ClaimTypes.Role                
            });
        }

        [HttpGet("User")]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var users = await FetchUsers();
            return Ok(users);
        }

        [HttpGet("Public")]
        public async Task<IActionResult> GetUsersPublic()
        {
            var users = await FetchUsers();
            return Ok(users);
        }
    }
}
