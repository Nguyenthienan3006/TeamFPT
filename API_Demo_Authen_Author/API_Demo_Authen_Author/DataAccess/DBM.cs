using API_Demo_Authen_Author.Services;
using MySqlConnector;
using System.Data;

namespace API_Demo_Authen_Author.DataAccess
{
    public class DBM
    {
        private readonly IDataService _dataService;

        public DBM(IDataService dataService)
        {
            _dataService = dataService;
        }

        public int InsertLoginLog(int userId)
        {
            string query = "INSERT INTO LoginLogs (UserId) VALUES (@UserId)";
            var parameters = new[] { new MySqlParameter("@UserId", userId) };

            return ExecuteNonQuery(query, parameters);
        }

        public DataTable ExecuteQuery(string query, params MySqlParameter[] parameters)
        {
            using var connection = _dataService.GetConnection();
            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using var adapter = new MySqlDataAdapter(command);
            var dataTable = new DataTable();
            connection.Open();
            adapter.Fill(dataTable);
            return dataTable;
        }

        public int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            using var connection = _dataService.GetConnection();
            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            using var connection = _dataService.GetConnection();
            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            return command.ExecuteScalar();
        }
    }
}
