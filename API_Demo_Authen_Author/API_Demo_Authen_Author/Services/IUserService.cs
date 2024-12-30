using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;

namespace API_Demo_Authen_Author.Services
{
    public interface IUserService
    {
        User Authenticate(LoginDto userLogin);
        bool RegisterUser(string token, RegisterDto registerDto);
        Task<List<UserDto>> FetchUsersAsync();
        Task<bool> VerifyEmailAsync(string token, int userId, string email);
        UserDto GetUserByEmail(string email);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPassword);
        string GenerateRandomPassword(int length);
    }
}
