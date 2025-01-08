using System.Linq.Expressions;
using StackExchange.Redis;

namespace Project_Swagger.Services
{
    public class RedisService
    {
        private static ConnectionMultiplexer redisConnection;
        private static IDatabase redisDatabase;
        public static void Connect(string connectionString)
        {
            redisConnection = ConnectionMultiplexer.Connect(connectionString);
            redisDatabase = redisConnection.GetDatabase();
        }
        public static void SetAccessToken(int id, string token, TimeSpan expiration){
            string key = Convert.ToString(id);
            redisDatabase.StringSet(key, token, expiration);
        }
        public static string GetAccessToken(string key)
        {
            string? redisInfo = redisDatabase.StringGet(key);
            return redisInfo;
        }
        public static void DeleteAccessToken(string key){ redisDatabase.KeyDelete(key);}
    }
}
