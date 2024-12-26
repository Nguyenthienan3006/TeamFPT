using UserManagementAPI.Models;
using MySql.Data.MySqlClient;
using System.Data;
using UserManagementAPI.Models;
using System.Configuration;

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

}
