using LoginProject.Data;
using LoginProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Data;

namespace LoginProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public UsersController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpGet("{username}")]
        [Authorize]
        public IActionResult GetUserByUsername(string username)
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
                return Ok(new User
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Username = reader["username"].ToString(),
                    Password = reader["password"].ToString(),
                    Email = reader["email"].ToString(),
                    Role = reader["role"].ToString()
                });
            }

            return NotFound();
        }
    }
}
