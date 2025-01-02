using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using API_Demo_Authen_Author.Dto;
using System.Security.Claims;
using API_Demo_Authen_Author.Services;

namespace API_Demo_Authen_Author.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet("Admin")]
        [Authorize(Roles = "admin")]
        public IActionResult AdminGetUsers()
        {
            var users = _userService.FetchUsers();
            return Ok(new
            {
                Name = User.FindFirstValue(ClaimTypes.Name),
                Role = User.FindFirstValue(ClaimTypes.Role),
                Users = users
            });
        }

        [HttpGet("User")]
        [Authorize]
        public IActionResult GetUsers()
        {
            var users = _userService.FetchUsers();
            return Ok(users);
        }

        [HttpGet("Public")]
        public IActionResult GetUsersPublic()
        {
            var users = _userService.FetchUsers();
            return Ok(users);
        }
    }
}
