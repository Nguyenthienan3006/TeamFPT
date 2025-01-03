using UserManagementAPI.Models;
using MySql.Data.MySqlClient;
using System.Data;
using UserManagementAPI.Models;
using System.Configuration;
using System.Security.Cryptography;

namespace UserManagementAPI.Data;

public class UserStore
{
    private readonly string _connectionString;
    //private readonly MySqlConnection _connection;

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
        if (reader.Read())
        {
            return reader.GetInt32("user_count") > 0;
        }

        return false;
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


    public string GenerateOtp(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var otp = new Random().Next(100000, 999999).ToString();
        //sp_GenerateSaveOTP
        using var command = new MySqlCommand("sp_GenerateSaveOTP", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_user_id", userId); 
        command.Parameters.AddWithValue("p_otp", otp);
        command.ExecuteNonQuery();
        return otp;
    }




    public bool ValidateOtp(int userId, string otp)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        //sp_ValidateOTP
        using var command = new MySqlCommand("sp_ValidateOTP", connection )
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_user_id", userId); 
        command.Parameters.AddWithValue("p_otp", otp);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var tokenId = reader.GetInt32("id");
            MarkOtpAsUsed(tokenId);
            return true;
        }

        return false;
    }

    public void SaveToken(int userId, string token)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand("sp_SaveToken", connection) { CommandType = System.Data.CommandType.StoredProcedure };
        command.Parameters.AddWithValue("p_user_id", userId);
        command.Parameters.AddWithValue("p_otp", token);

        command.ExecuteNonQuery();
    }
    private void MarkOtpAsUsed(int tokenId)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = new MySqlCommand("sp_IsUseOTP", connection) { CommandType = System.Data.CommandType.StoredProcedure };
        command.Parameters.AddWithValue("p_id", tokenId);
        command.ExecuteNonQuery();
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




}
