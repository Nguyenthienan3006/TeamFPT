using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;

namespace API_Demo_Authen_Author.Services
{
    public interface IUserService
    {
        User Authenticate(LoginDto userLogin);
        Task<bool> RegisterUserAsync(RegisterDto userRegister, string token);
        Task<List<UserDto>> FetchUsersAsync();
        Task<bool> VerifyEmailAsync(string token, int userId, string email);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPassword);
        string GenerateRandomPassword(int length);
    }
}
