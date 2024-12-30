using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;

namespace API_Demo_Authen_Author.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        bool UpdateToken(int userId, string token, string tokenType, DateTime expiredDate, bool isUsed);
    }
}
