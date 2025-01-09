using MySql.Data.MySqlClient;

namespace TeamFPT.Services
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;
        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }
        public MySqlConnection GetConnection() 
        { 
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
        }
    }
}
