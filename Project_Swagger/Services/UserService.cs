using System.Data;
using MySql.Data.MySqlClient;
using Project_Swagger.DTO;
using Project_Swagger.Models;

namespace Project_Swagger.Services
{
    public class UserService
    {
        private readonly MySqlConnectionService _connectionService;

        public UserService(MySqlConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public User GetAnUser(string username, string password)
        {
            using var connection = _connectionService.GetConnection();

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "FindUserByUsername"; 
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("@un", MySqlDbType.VarChar) { Value = username });

            using var reader = command.ExecuteReader();

            if (reader.Read()) 
            {
               
                if (password.Equals(reader["Password"].ToString()))  
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["user_id"]), 
                        Username = reader["Username"].ToString(),   
                        Email = reader["Email"].ToString(),         
                        Role = reader["Role"].ToString()            
                    };
                }
            }

            return null;
        }

        public string AddAnUser(UserRegisterDTO userRegisterDTO)
        {
            string result = null;


            return result;
        }
    }
}
