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


    public async Task<bool> UserExistsAsync(string username)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand("sp_CheckUserExists", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader.GetInt32("user_count") > 0;
        }

        return false;
    }

    public async Task AddUserAsync(User user)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand("sp_AddUser", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_username", user.Username);
        command.Parameters.AddWithValue("p_password", user.Password);
        command.Parameters.AddWithValue("p_email", user.Email);
        command.Parameters.AddWithValue("p_role", user.Role);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("CALL sp_GetUserByUsername(@Username);", connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
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


    public async Task<string> GenerateOtpAsync(int userId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var otp = new Random().Next(100000, 999999).ToString();
        //sp_GenerateSaveOTP
        using var command = new MySqlCommand("sp_GenerateSaveOTP", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_user_id", userId); 
        command.Parameters.AddWithValue("p_otp", otp);
        await command.ExecuteNonQueryAsync();
        return otp;
    }




    public async Task<bool> ValidateOtpAsync(int userId, string otp)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        //sp_ValidateOTP
        using var command = new MySqlCommand(
            "sp_ValidateOTP", connection
        )
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("p_user_id", userId); 
        command.Parameters.AddWithValue("p_otp", otp);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var tokenId = reader.GetInt32("id");
            await MarkOtpAsUsedAsync(tokenId);
            return true;
        }

        return false;
    }

    private async Task MarkOtpAsUsedAsync(int tokenId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand("sp_IsUseOTP", connection) { CommandType = System.Data.CommandType.StoredProcedure };
        command.Parameters.AddWithValue("p_id", tokenId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string newPassword)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        //sp_UpdatePassword
        using var command = new MySqlCommand("sp_UpdatePassword", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        }; ;
        command.Parameters.AddWithValue("p_user_id", userId);
        command.Parameters.AddWithValue("p_NewPassword", newPassword);

        await command.ExecuteNonQueryAsync();
    }




}
