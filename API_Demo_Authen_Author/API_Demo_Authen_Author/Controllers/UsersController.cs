using API_Demo_Authen_Author.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_Demo_Authen_Author.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public UsersController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpGet("Admin")]
        [Authorize(Roles = "admin")]
        public IActionResult AdminGetUsers()
        {
            if (_tokenService.GetTokenFromRedisAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))) == null) return BadRequest("Token not found");

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
            // Lấy ra token từ Redis
            if (_tokenService.GetTokenFromRedisAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))) == null) return BadRequest("Token not found");

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
