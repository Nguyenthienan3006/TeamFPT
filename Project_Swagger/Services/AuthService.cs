using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Project_Swagger.Models;
using Project_Swagger.DTO;

namespace Project_Swagger.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthService(UserService userService, IConfiguration config)
        {
            _userService = userService;
            _configuration = config;
        }


        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Issuer = _configuration["Jwt:Issuer"],
                Expires = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:TokenValidityMins")),
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }

        public User Authenticate(UserDTO userLogin)
        {
            var currentUser = _userService.GetAnUser(userLogin.UserName, userLogin.PassWord);
            if (currentUser != null)
            {
                return currentUser;
            }
            return null;
        }
    }
}
