using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;
using Dapper;
using MySqlConnector;
using System.Data;
using System.Security.Cryptography;

namespace API_Demo_Authen_Author.Services
{
    public class UserService : IUserService
    {
        private readonly IDataService _dataService;
        private readonly ITokenService _tokenService;

        public UserService(IDataService dataService, ITokenService tokenService)
        {
            _dataService = dataService;
            _tokenService = tokenService;
        }

        public User Authenticate(LoginDto userLogin)
        {
            using var connection = _dataService.GetConnection();
            return connection.QueryFirstOrDefault<User>(
                "sp_Login", new { p_username = userLogin.UserName }, commandType: CommandType.StoredProcedure
            );
        }

        public List<UserDto> FetchUsers()
        {
            using var connection = _dataService.GetConnection();

            return connection.Query<UserDto>(
                "sp_GetAllUsers",
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        public bool RegisterUser(string token, RegisterDto userRegister, byte[] passwordHash, byte[] passwordSalt)
        {
            using var connection = _dataService.GetConnection();

            // Gọi stored procedure để thêm user
            var result = connection.Execute(
                "sp_RegisterUser",
                new
                {
                    p_username = userRegister.UserName,
                    p_passwordHash = passwordHash,
                    p_passwordSalt = passwordSalt,
                    p_email = userRegister.Email
                },
                commandType: CommandType.StoredProcedure
            );

            //Lấy IdUser
            var user = GetUserByEmail(userRegister.Email);
            if (user == null) return false;

            // Cập nhật token vào DB
            _tokenService.UpdateToken(user.UserId, token, "EmailToken", DateTime.Now.AddMinutes(30), false);

            return result > 0 ? true : false;
        }

        public UserDto GetUserByEmail(string email)
        {
            using var connection = _dataService.GetConnection();
            return connection.QueryFirstOrDefault<UserDto>(
                "sp_GetUserByEmail",
                new { p_email = email },
                commandType: CommandType.StoredProcedure
            );
        }

        public bool VerifyEmail(string token, int userId, string email)
        {
            using var connection = _dataService.GetConnection();
            var result = connection.Execute(
                "sp_VerifyEmail",
                new
                {
                    p_token = token,
                    p_email = email,
                    p_userId = userId
                },
                commandType: CommandType.StoredProcedure
            );
            return result > 0;
        }

        public bool UpdateUserPassword(int userId, string newPassword)
        {
            using var connection = _dataService.GetConnection();
            var result = connection.Execute(
                "sp_UpdateUserPassword",
                new
                {
                    p_user_id = userId,
                    p_new_password = newPassword
                },
                commandType: CommandType.StoredProcedure
            );
            return result > 0;
        }
        public string GenerateRandomPassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+[]{}|;:,.<>?";
            var random = new Random();
            return new string(new char[length].Select(c => chars[random.Next(chars.Length)]).ToArray());
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}

