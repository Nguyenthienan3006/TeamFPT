using API_Demo_Authen_Author.Dto;
using API_Demo_Authen_Author.Models;
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

            using var command = new MySqlCommand("sp_Login", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_username", userLogin.UserName);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role"),
                    IsEmailVerified = reader.GetBoolean("IsEmailVerified"),
                    passwordHash = (byte[])reader["passwordHash"],
                    passwordSalt = (byte[])reader["passwordSalt"]
                };
            }

            return null;
        }

        public List<UserDto> FetchUsers()
        {
            using var connection = _dataService.GetConnection();

            var users = new List<UserDto>();

            using var command = new MySqlCommand("sp_GetAllUsers", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();
            while (reader.Read())
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

        public bool RegisterUser(string token, RegisterDto userRegister, byte[] passwordHash, byte[] passwordSalt)
        {
            using var connection = _dataService.GetConnection();

            //Cập nhật user vào DB
            using var command = new MySqlCommand("sp_RegisterUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_username", userRegister.UserName);
            command.Parameters.AddWithValue("p_passwordHash", passwordHash);
            command.Parameters.AddWithValue("p_passwordSalt", passwordSalt);
            command.Parameters.AddWithValue("p_email", userRegister.Email);

            var result1 = command.ExecuteNonQuery();

            //Lấy IdUser
            var user = GetUserByEmail(userRegister.Email);
            if (user == null) return false;

            // Cập nhật token vào DB
            _tokenService.UpdateToken(user.Id, token, "EmailToken", DateTime.Now.AddMinutes(30), false);

            return result1 > 0 ? true : false;
        }

        public UserDto GetUserByEmail(string email)
        {
            using var connection = _dataService.GetConnection();

            using var command = new MySqlCommand("sp_GetUserByEmail", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_email", email);

            using var reader = command.ExecuteReader();
            if (reader.Read())
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

        public bool VerifyEmail(string token, int userId, string email)
        {
            using var connection = _dataService.GetConnection();

            using var command = new MySqlCommand("sp_VerifyEmail", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_token", token);
            command.Parameters.AddWithValue("p_email", email);
            command.Parameters.AddWithValue("p_userId", userId);


            var result = command.ExecuteNonQuery();
            return result > 0;
        }

        public bool UpdateUserPassword(int userId, string newPassword)
        {
            using var connection = _dataService.GetConnection();

            using var command = new MySqlCommand("sp_UpdateUserPassword", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Thêm các tham số cho stored procedure
            command.Parameters.AddWithValue("p_user_id", userId);
            command.Parameters.AddWithValue("p_new_password", newPassword);

            // Thực thi stored procedure
            var result = command.ExecuteNonQuery();

            // Kiểm tra kết quả và trả về true nếu thành công
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

