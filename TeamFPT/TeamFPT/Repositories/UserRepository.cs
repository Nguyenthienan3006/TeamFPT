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
                    UserRole = reader.GetString("UserRole"),
                    User = user
                };
            }
            return null;
        }

        public void Register(Users user)
        {
          
        }
     
    }
}
