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
        public async Task<IActionResult> AdminGetUsers()
        {
            var users = await _userService.FetchUsersAsync();
            return Ok(new
            {
                Name = User.FindFirstValue(ClaimTypes.Name),
                Role = User.FindFirstValue(ClaimTypes.Role),
                Users = users
            });
        }

        [HttpGet("User")]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.FetchUsersAsync();
            return Ok(users);
        }

        [HttpGet("Public")]
        public async Task<IActionResult> GetUsersPublic()
        {
            var users = await _userService.FetchUsersAsync();
            return Ok(users);
        }
    }
}
