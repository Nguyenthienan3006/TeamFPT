using Microsoft.Extensions.Caching.Distributed;

namespace TeamFPT.Services
{
    public class RedisServices
    {
        private readonly IDistributedCache _cache;
        public RedisServices(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task SaveTokenToRedisAsync(string token, int userId)
        {
            var cacheKey = $"jwt:{userId}";
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            await _cache.SetStringAsync(cacheKey, token, cacheOptions);
        }
        public async Task<string?> GetTokenFromRedisAsync(int userId)
        {
            var cacheKey = $"jwt:{userId}";
            return await _cache.GetStringAsync(cacheKey);
        }

        public async Task RemoveTokenFromRedisAsync(int userId)
        {
            var cacheKey = $"jwt:{userId}";
            await _cache.RemoveAsync(cacheKey);
        }

    }
}
