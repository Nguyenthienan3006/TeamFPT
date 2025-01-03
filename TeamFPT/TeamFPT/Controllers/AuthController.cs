using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamFPT.DTO;
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
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest request)
        {
            var userAuth = _userRepositories.Login(request.Username, request.Password);
            if (userAuth == null) return Unauthorized("Invalid credentials.");
            var token = _jwtTokenGenerator.GenerateToken(userAuth);
            return Ok(new
            {
                token,
                user = new
                {
                    userAuth.Username,
                    userAuth.Email,
                    userAuth.UserRole,
                    userAuth.User.FirstName,
                    userAuth.User.LastName
                }
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Users user)
        {
            return null;
        }


    }
}
