using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;

namespace API_Demo_Authen_Author.Services
{
    public interface IUserService
    {
        User Authenticate(LoginDto userLogin);
        bool RegisterUser(string token, RegisterDto registerDto, byte[] passwordHash, byte[] passwordSalt);
        List<UserDto> FetchUsers();
        bool VerifyEmail(string token, int userId, string email);
        UserDto GetUserByEmail(string email);
        bool UpdateUserPassword(int userId, string newPassword);
        string GenerateRandomPassword(int length);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);

    }
}
