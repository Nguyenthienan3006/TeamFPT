using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System.Text.Json.Serialization;

namespace LoginProject.Services
{
    public class RedisService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
        {
            _distributedCache = cache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task SetCacheStringAsync(string key, string value, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _distributedCache.SetStringAsync(key, value, options);
        }

        public async Task<string?> GetCacheAsync(string key)
        {
            return await _distributedCache.GetStringAsync(key);
        }

        public async Task RemoveCacheAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task<string?> GetCacheResponseAsync(string cacheKey)
        {
            var cacheResponse = await _distributedCache.GetStringAsync(cacheKey);
            return string.IsNullOrEmpty(cacheResponse) ? null : cacheResponse;
        }

        public async Task SetCacheResponseAsync(string cacheKey, object response, TimeSpan expiration)
        {
            if (response == null) return;

            var serializerResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await _distributedCache.SetStringAsync(cacheKey, serializerResponse, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });

        }
    }
}
