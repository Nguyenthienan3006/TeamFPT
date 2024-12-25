using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Demo_Authen_Author.Models;
using Microsoft.AspNetCore.Authorization;
using API_Demo_Authen_Author.Dto;
using MySqlConnector;

namespace API_Demo_Authen_Author.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DemoAPIContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(DemoAPIContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Users
        [HttpGet("Admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> AdminGetUsers()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            var users = new List<UserDto>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("GetAllUsers", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new UserDto
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString("FullName"),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                                Role = reader.GetString("Role")
                            });
                        }
                    }
                }
            }

            return Ok(users);
        }
        



        [HttpGet("User")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            var users = new List<UserDto>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("GetAllUsers", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new UserDto
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString("FullName"),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                                Role = reader.GetString("Role")
                            });
                        }
                    }
                }
            }

            return Ok(users);
        }
        
        
        [HttpGet("Public")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersPublic()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            var users = new List<UserDto>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("GetAllUsers", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new UserDto
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString("FullName"),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                                Role = reader.GetString("Role")
                            });
                        }
                    }
                }
            }

            return Ok(users);
        }




    }
}
