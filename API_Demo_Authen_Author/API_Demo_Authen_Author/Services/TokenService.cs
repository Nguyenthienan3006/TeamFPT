using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Demo_Authen_Author.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IDataService _dataService;
        private readonly IDistributedCache _cache;

        public TokenService(IConfiguration config, IDataService dataService, IDistributedCache cache)
        {
            _config = config;
            _dataService = dataService;
            _cache = cache;
        }

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Issuer = _config["Jwt:Issuer"],
                Expires = DateTime.Now.AddMinutes(_config.GetValue<int>("Jwt:TokenValidityMins")),
                Audience = _config["Jwt:Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }

        public TokenInfoDto GetTokenInfo(int userId, string tokenType)
        {
            try
            {
                using var connection = _dataService.GetConnection();

                using var command = new MySqlCommand("sp_GetToken", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("p_UserId", userId);
                command.Parameters.AddWithValue("p_TokenType", tokenType);

                using var reader = command.ExecuteReader();
                if (reader.Read() == false) return null;

                return new TokenInfoDto
                {
                    token = reader.GetString("Token"),
                    expiredDate = reader.GetDateTime("ExpirationDate")
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task SaveTokenToRedisAsync(string token, int userId)
        {
            // Key định danh token cho user
            var cacheKey = $"jwt:{userId}"; 
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) 
            };
            await _cache.SetStringAsync(cacheKey, token, cacheOptions);
        }

        public bool UpdateToken(int userId, string token, string tokenType, DateTime expiredDate, bool isUsed)
        {
            try
            {
                using var connection = _dataService.GetConnection();

                // Gọi stored procedure bằng Dapper
                var result = connection.Execute(
                    "sp_UpdateToken",
                    new
                    {
                        p_UserId = userId,
                        p_Token = token,
                        p_TokenType = tokenType,
                        p_ExpirationDate = expiredDate,
                        p_IsUsed = isUsed
                    },
                    commandType: CommandType.StoredProcedure
                );

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }


    }
}
