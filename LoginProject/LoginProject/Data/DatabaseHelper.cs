using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace LoginProject.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public bool ExecuteStoredProcedure(string storedProcedureName, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                int rowsAffected = connection.Execute(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
                return rowsAffected > 0;
            }
        }

        public IEnumerable<T> ExecuteStoredProcedure<T>(string storedProcedureName, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.Query<T>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public T? ExecuteStoredProcedureSingle<T>(string storedProcedureName, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();   
                return connection.QueryFirstOrDefault<T>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
