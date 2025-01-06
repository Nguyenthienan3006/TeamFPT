using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TeamFPT.DTO;
using TeamFPT.Model;

namespace TeamFPT.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        public UserAuthentication Login(string username, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
          connection.Open();

            using var command = new MySqlCommand("Login", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_Username", username);
            command.Parameters.AddWithValue("@p_Password", password);

            using var reader =  command.ExecuteReader();
            if (reader.Read())
            {
                var user = new Users
                {
                    UserId = reader.GetInt32("UserId"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName")
                };

                return new UserAuthentication
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    IsVerified = reader.GetBoolean("IsVerified"),
                    UserRole = reader.GetString("UserRole"),
                    User = user
                };
            }
            return null;
        }
        public void Register(RegisterRequest request)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = new MySqlCommand("Register", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@p_FirstName", request.FirstName);
            command.Parameters.AddWithValue("@p_LastName", request.LastName);
            command.Parameters.AddWithValue("@p_Address", request.Address);
            command.Parameters.AddWithValue("@p_Email", request.Email);
            command.Parameters.AddWithValue("@p_Username", request.Username);
            command.Parameters.AddWithValue("@p_Password", request.Password);
            command.Parameters.AddWithValue("@p_UserRole", request.UserRole);
            command.ExecuteNonQuery();
        }
        public void SaveOtp(string email, string otp)
        {
            CleanupExpiredOtp();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var command = new MySqlCommand(
                "INSERT INTO Tokens (UserId, TokenType, TokenValue, Expiration) " +
                "SELECT UserId, 'OTP', @Otp, DATE_ADD(NOW(), INTERVAL 20 SECOND) FROM UserAuthentication WHERE Email = @Email", connection);
            command.Parameters.AddWithValue("@Otp", otp);
            command.Parameters.AddWithValue("@Email", email);

            command.ExecuteNonQuery();
        }

        private bool IsUniqueEmail(string email)
        {
            using var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
            connection.Open();

            var command = new MySql.Data.MySqlClient.MySqlCommand(
                "SELECT COUNT(*) FROM UserAuthentication WHERE Email = @Email", connection);
            command.Parameters.AddWithValue("@Email", email);

            return (long)command.ExecuteScalar() == 0;
        }
        public ValidationResults ValidateUser(RegisterRequest user)
        {
            var result = new ValidationResults();

            if (string.IsNullOrWhiteSpace(user.Email) || !new EmailAddressAttribute().IsValid(user.Email))
                result.Errors.Add("Invalid email format.");

            if (user.Username.Length < 5 || user.Username.Length > 50)
                result.Errors.Add("Username must be between 5 and 50 characters.");

            if (!IsUniqueEmail(user.Email))
                result.Errors.Add("Email is already registered.");

            if (!IsUniqueUsername(user.Username))
                result.Errors.Add("Username is already taken.");

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        public string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
        private bool IsUniqueUsername(string username)
        {
            using var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
            connection.Open();

            var command = new MySql.Data.MySqlClient.MySqlCommand(
                "SELECT COUNT(*) FROM UserAuthentication WHERE Username = @Username", connection);
            command.Parameters.AddWithValue("@Username", username);

            return (long)command.ExecuteScalar() == 0;
        }
        public bool VerifyOtp(string email, string otp)
        {
            using var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
            connection.Open();

            var command = new MySql.Data.MySqlClient.MySqlCommand(
                "SELECT u.UserId FROM UserAuthentication u " +
                "JOIN Tokens t ON u.UserId = t.UserId " +
                "WHERE u.Email = @Email AND t.TokenValue = @Otp AND t.Expiration > NOW()", connection);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@OTP", otp);
            var userId = command.ExecuteScalar();

            if (userId != null)
            {
                var updateCommand = new MySql.Data.MySqlClient.MySqlCommand(
                    "UPDATE UserAuthentication SET IsVerified = 1 WHERE UserId = @UserId", connection);
                updateCommand.Parameters.AddWithValue("@UserId", userId);
                updateCommand.ExecuteNonQuery();
                var deleteCommand = new MySqlCommand(
           "DELETE FROM Tokens WHERE TokenValue = @Otp", connection);
                deleteCommand.Parameters.AddWithValue("@Otp", otp);
                deleteCommand.ExecuteNonQuery();
                return true;
            }

            return false;
        }
        public void CleanupExpiredOtp()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var command = new MySqlCommand(
                "DELETE FROM Tokens WHERE Expiration < NOW()", connection);
            command.ExecuteNonQuery();
        }
        public void ChangePassword(int userId, string oldPassword, string newPassword)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var command = new MySqlCommand("ChangePassword", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_UserId", userId);
            command.Parameters.AddWithValue("@p_OldPassword", oldPassword);
            command.Parameters.AddWithValue("@p_NewPassword", newPassword);

            command.ExecuteNonQuery();
        }
    }
}
