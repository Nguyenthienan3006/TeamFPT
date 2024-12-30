using MySqlConnector;

namespace API_Demo_Authen_Author.Services
{
    public class DataService : IDataService
    {
        private readonly IConfiguration _config;

        public DataService(IConfiguration config)
        {
            _config = config;
        }
        public MySqlConnection GetConnection()
        {
            try
            {
                var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to database: {ex.Message}");
                return null; // Trả về false nếu không kết nối được
            }
        }

    }
}
