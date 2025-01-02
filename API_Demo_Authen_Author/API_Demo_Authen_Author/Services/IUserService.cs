using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;

namespace API_Demo_Authen_Author.Services
{
    public interface IUserService
    {
        User Authenticate(LoginDto userLogin);
        bool RegisterUser(string token, RegisterDto registerDto);
        List<UserDto> FetchUsers();
        bool VerifyEmail(string token, int userId, string email);
        UserDto GetUserByEmail(string email);
        bool UpdateUserPassword(int userId, string newPassword);
        string GenerateRandomPassword(int length);

        bool HasValidPasswordFormat(string password);
    }
}
