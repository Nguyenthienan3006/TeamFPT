using Microsoft.Extensions.Caching.Distributed;

namespace LoginProject.Services
{
    public class RedisService
    {
        private readonly IDistributedCache _cache;

        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetCacheAsync(string key, string value, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(key, value, options);
        }

        public async Task<string?> GetCacheAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task RemoveCacheAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
