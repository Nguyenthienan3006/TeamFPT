using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamFPT.Model;
using TeamFPT.Repositories;
using TeamFPT.Services;

namespace TeamFPT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserRepository _userRepositories;
        private readonly IConfiguration _configuration;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public AuthController(UserRepository userRepositories, IConfiguration configuration, JwtTokenGenerator jwtTokenGenerator)
        {
            _userRepositories = userRepositories;
            _configuration = configuration;
            _jwtTokenGenerator = jwtTokenGenerator;
        }
        [HttpGet("userinfo")]
        [Authorize] // Bắt buộc phải có JWT token hợp lệ
        public IActionResult GetUserInfo()
        {
            // Lấy thông tin từ claim của JWT token
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Token không hợp lệ.");

            var user = _userRepositories.GetUserByUsername(username);
            if (user == null)
                return NotFound("Người dùng không tồn tại.");

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.Role
            });
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest request)
        {
            var user = _userRepositories.Login(request.Username, request.Password);
            if (user == null) return Unauthorized("Invalid credentials.");

            var token = _jwtTokenGenerator.GenerateToken(user);
            return Ok(token);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            _userRepositories.Register(user);
            return Ok("Registration successful.");
        }


    }
}
