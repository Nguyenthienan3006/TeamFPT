using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;
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

        public TokenService(IConfiguration config, IDataService dataService)
        {
            _config = config;
            _dataService = dataService;
        }

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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


        public bool UpdateToken(int userId, string token, string tokenType, DateTime expiredDate, bool isUsed)
        {

            try
            {
                using var connection = _dataService.GetConnection();

                using var command = new MySqlCommand("sp_UpdateToken", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("p_UserId", userId);
                command.Parameters.AddWithValue("p_Token", token);
                command.Parameters.AddWithValue("p_TokenType", tokenType);
                command.Parameters.AddWithValue("p_ExpirationDate", expiredDate);
                command.Parameters.AddWithValue("p_IsUsed", isUsed);

                var result = command.ExecuteNonQuery();
                return result > 0 ? true : false;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}
