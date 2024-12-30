using MySqlConnector;

namespace API_Demo_Authen_Author.Services
{
    public interface IDataService
    {
        MySqlConnection GetConnection();
    }
}
