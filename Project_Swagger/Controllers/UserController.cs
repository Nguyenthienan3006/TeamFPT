using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_Swagger.Models;
using Project_Swagger.Services;
using Microsoft.IdentityModel.Tokens;

namespace Project_Swagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/<UserController>
        [HttpGet("Admin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<User>>> AdminGetInf()
        {
            var _user = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (_user.IsNullOrEmpty())
            {
                return Unauthorized("User not authenticated.");
            }

            // Giả sử bạn có phương thức này trong service để lấy thông tin người dùng theo tên người dùng
            if (_user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                name = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                role = User?.FindFirstValue(ClaimTypes.Role),
                email = User?.FindFirstValue(ClaimTypes.Email)
            });
        }

        [HttpGet("User")]
        [Authorize(Roles = "user")]
        public async Task<ActionResult<IEnumerable<User>>> UserGetInf()
        {
            var _user = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (_user.IsNullOrEmpty())
            {
                return Unauthorized("User not authenticated.");
            }

            // Giả sử bạn có phương thức này trong service để lấy thông tin người dùng theo tên người dùng
            if (_user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                name = ClaimTypes.NameIdentifier.ToString(),
                role = ClaimTypes.Role.ToString(),
                email = ClaimTypes.Email.ToString()
            });
        }

        [HttpGet("Publice")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Public()
        {
            var _user = "Must login";

            return Ok(_user);
        }
    }
}
