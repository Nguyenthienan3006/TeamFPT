using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

namespace API_Demo_Authen_Author.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        public User Authenticate(LoginDto userLogin)
        {
            using var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var command = new MySqlCommand("sp_Login", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_username", userLogin.UserName);
            command.Parameters.AddWithValue("p_password", userLogin.PassWord);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role")
                };
            }

            return null;
        }

        public async Task<List<UserDto>> FetchUsersAsync()
        {
            var users = new List<UserDto>();
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_GetAllUsers", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new UserDto
                {
                    Id = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role"),
                    IsEmailVerified = reader.GetBoolean("IsEmailVerified")
                });
            }

            return users;
        }

        public async Task<bool> RegisterUserAsync(RegisterDto userRegister, string token)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_RegisterUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_username", userRegister.UserName);
            command.Parameters.AddWithValue("p_password", userRegister.PassWord);
            command.Parameters.AddWithValue("p_email", userRegister.Email);
            command.Parameters.AddWithValue("p_token", token);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_GetUserByEmail", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserDto
                {
                    Id = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role")
                };
            }

            return null;
        }

        public async Task<bool> VerifyEmailAsync(string token, int userId, string email)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_VerifyEmail", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_token", token);
            command.Parameters.AddWithValue("p_email", email);
            command.Parameters.AddWithValue("p_userId", userId);


            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string newPassword)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_UpdateUserPassword", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Thêm các tham số cho stored procedure
            command.Parameters.AddWithValue("p_user_id", userId);
            command.Parameters.AddWithValue("p_new_password", newPassword);

            // Thực thi stored procedure
            var result = await command.ExecuteNonQueryAsync();

            // Kiểm tra kết quả và trả về true nếu thành công
            return result > 0;
        }
        public string GenerateRandomPassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+[]{}|;:,.<>?";
            var random = new Random();
            return new string(new char[length].Select(c => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}

