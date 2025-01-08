using LoginProject.Attributes;
using LoginProject.DTO;
using LoginProject.Repositories;
using LoginProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoginProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly JwtService _jwtService;

        public UsersController(UsersService usersService, JwtService jwtService)
        {
            _usersService = usersService;
            _jwtService = jwtService;
        }

        [HttpGet("get-list-users")]
        [Cache(1000)]
        public IActionResult GetAllUsers([FromQuery] PagingModel pagingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var users = _usersService.GetPagedUsers(pagingModel.PageNumber, pagingModel.PageSize);
            if (users == null) return NotFound("No users found");

            var totalCount = _usersService.GetTotalUserCount();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pagingModel.PageSize);
            var result = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = users
            };
            return Ok(result);
        }

        [HttpGet("{username}")]
        [Authorize]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            if (!await _jwtService.CheckTokenRedis(User.FindFirstValue(ClaimTypes.NameIdentifier))) return Unauthorized();
            var user = _usersService.GetUserByUsername(username);
            if (user == null) return NotFound();

            return Ok(user);
        }
    }
}
