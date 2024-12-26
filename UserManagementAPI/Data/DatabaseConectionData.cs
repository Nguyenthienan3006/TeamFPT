using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
namespace UserManagementAPI.Data;

public class DatabaseConectionData
{
    private readonly string _connectionString;

    public DatabaseConectionData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public IDbConnection Connection() => new MySqlConnection(_connectionString);
}
