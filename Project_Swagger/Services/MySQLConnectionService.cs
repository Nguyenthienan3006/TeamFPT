using MySql.Data.MySqlClient;
using System.Data;

namespace Project_Swagger.Services
{
    public class MySqlConnectionService
    {
        private readonly string _connectionString;

        public MySqlConnectionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection GetConnection() => new MySqlConnection(_connectionString);
    }
}
