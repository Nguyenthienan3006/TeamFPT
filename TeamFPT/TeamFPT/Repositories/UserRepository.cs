using MySql.Data.MySqlClient;
using System.Data;
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
        public User Login(string username, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand("sp_Login", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("p_username", username);
            command.Parameters.AddWithValue("p_password", password);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role")
                };
            }
            return null;
        }

        public void Register(User user)
        {
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand("sp_Register", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("p_username", user.Username);
            command.Parameters.AddWithValue("p_password", user.Password);
            command.Parameters.AddWithValue("p_email", user.Email);
            command.Parameters.AddWithValue("p_role", user.Role);

            connection.Open();
            command.ExecuteNonQuery();
        }
        public User? GetUserByUsername(string username)
        {
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand("sp_GetUserByUsername", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_username", username);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Role = reader.GetString("role")
                };
            }

            return null;
        }

    }
}
