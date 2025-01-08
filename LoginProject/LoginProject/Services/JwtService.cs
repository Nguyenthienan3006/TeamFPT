
using LoginProject.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace LoginProject.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly RedisService _redisService;
        public JwtService(IConfiguration configuration, RedisService redisService)
        {
            _config = configuration;
            _redisService = redisService;
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Issuer = _config["Jwt:Issuer"],
                Expires = DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:TokenValidityMins")),
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var redisKey = $"jwt_{user.UserId}";
            await _redisService.SetCacheStringAsync(redisKey, tokenString, TimeSpan.FromMinutes(30));
            return tokenString;
        }


        public async Task<bool> CheckTokenRedis(string? userId)
        {
            return await _redisService.GetCacheAsync($"jwt_{userId}") != null;
        }
    }


}
