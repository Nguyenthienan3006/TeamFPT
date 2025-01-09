using UserManagementAPI.Models;
using MySql.Data.MySqlClient;
using System.Data;
using UserManagementAPI.Models;
using System.Configuration;
using System.Security.Cryptography;
using UserManagementAPI.DTOs;
using System.Text.RegularExpressions;

namespace UserManagementAPI.Data;

public class UserStore
{
    private readonly string _connectionString;
    
    public UserStore(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }


    public bool UserExists(string username)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand("sp_CheckUserExists", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_username", username);

        using var reader = command.ExecuteReader();
        if (reader.Read()) return reader.GetInt32("user_count") > 0;

        return false;
    }

    public bool isValidPassWord(string password)
    {
        /*Regex regex = new Regex(@"^(.{0,7}|[^0-9]*|[^A-Z])$");
        
        return regex.IsMatch(password);*/
        //if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsDigit)) return false;

        return true;
    }

    public void AddUser(User user)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand("sp_AddUser", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_username", user.Username);
        command.Parameters.AddWithValue("p_password", user.Password);
        command.Parameters.AddWithValue("p_email", user.Email);
        command.Parameters.AddWithValue("p_role", user.Role);

        command.ExecuteNonQuery();
    }

    public User? GetUserByUsername(string username)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var command = new MySqlCommand("CALL sp_GetUserByUsername(@Username);", connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32("user_id"), 
                Username = reader.GetString("username"),
                Password = reader.GetString("password"),
                Email = reader.GetString("email"),
                Role = reader.GetString("role")
            };
        }

        return null;
    }

    public User? GetUserByEmail(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var command = new MySqlCommand("CALL sp_GetUserByUserEmail(@email);", connection);
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32("user_id"),
                Username = reader.GetString("username"),
                Password = reader.GetString("password"),
                Email = reader.GetString("email"),
                Role = reader.GetString("role")
            };
        }

        return null;
    }



    public string GenerateOtp(int userId, string type)
    {
        // Sinh OTP ngẫu nhiên (6 chữ số)
        var otp = new Random().Next(100000, 999999).ToString();

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        
        using var command = new MySqlCommand("sp_GenerateSaveOTP", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_user_id", userId);
        command.Parameters.AddWithValue("p_otp", otp);
        command.Parameters.AddWithValue("p_type", type); // Gửi loại OTP ('change_password' hoặc 'verify_email')

        command.ExecuteNonQuery();

        return otp; // Trả về OTP để gửi qua email
    }


    public void GenerateEmailVerificationOtp(int userId)
    {
        var otp = GenerateOtp(userId, "verify_email"); // Hàm sinh OTP
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        //sp_
        using var command = new MySqlCommand("sp_UpdateOTPVerifyEmail", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        }; 
        command.Parameters.AddWithValue("p_user_id", userId);
        command.Parameters.AddWithValue("p_otp", otp);
        command.ExecuteNonQuery();
    }


    public bool ValidateOtp(int userId, string otp, string type)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Gọi stored procedure sp_ValidateOTP
            using var command = new MySqlCommand("sp_ValidateOTP", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("p_user_id", userId);
            command.Parameters.AddWithValue("p_otp", otp);
            command.Parameters.AddWithValue("p_type", type);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var tokenId = reader.GetInt32("id");

                // Đánh dấu OTP là đã sử dụng
                MarkOtpAsUsed(tokenId);

                return true;
            }

            // Không tìm thấy OTP hợp lệ
            return false;
        }
        catch (Exception ex)
        {
            // Ghi log lỗi hoặc xử lý ngoại lệ
            Console.WriteLine($"Error validating OTP: {ex.Message}");
            return false;
        }
    }

/*    public void SaveToken(int userId, string token)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand("sp_SaveToken", connection) { CommandType = System.Data.CommandType.StoredProcedure };
        command.Parameters.AddWithValue("p_user_id", userId);
        command.Parameters.AddWithValue("p_otp", token);

        command.ExecuteNonQuery();
    }*/
    private void MarkOtpAsUsed(int tokenId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = new MySqlCommand("sp_IsUseOTP", connection) { CommandType = System.Data.CommandType.StoredProcedure };
            command.Parameters.AddWithValue("p_id", tokenId);
            command.ExecuteNonQuery();
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Error marking OTP as used: {ex.Message}");
        }
    }

    public void UpdatePassword(int userId, string newPassword)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        //sp_UpdatePassword
        using var command = new MySqlCommand("sp_UpdatePassword", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        }; 
        command.Parameters.AddWithValue("p_user_id", userId);
        command.Parameters.AddWithValue("p_NewPassword", newPassword);

        command.ExecuteNonQuery();
    }

    public List<UserDTO> GetUsers()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var users = new List<UserDTO>();
        using var command = new MySqlCommand("sp_GetAllUser", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserDTO
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

    public void MarkEmailAsVerified(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand("sp_changeVerifyEmail", connection) {CommandType = System.Data.CommandType.StoredProcedure };
        command.Parameters.AddWithValue("p_user_id", userId);
        command.ExecuteNonQuery();
    }

    public bool IsEmailVerified(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand("sp_isVerifyEmail", connection) {CommandType = System.Data.CommandType.StoredProcedure };
        command.Parameters.AddWithValue("p_user_id", userId);

        return Convert.ToBoolean(command.ExecuteScalar());
    }

}
