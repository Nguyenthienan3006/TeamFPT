using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace TeamFPT.Services
{
	public class RedisService
	{

		private readonly IDistributedCache _cache;

		public RedisService(IDistributedCache cache)
		{
			// Initialize Redis connection
			_cache = cache;
		}
		public async Task SaveTokenToRedisAsync(string token, int userId)
		{
			// Key định danh token cho user
			var cacheKey = $"jwt:{userId}";
			var cacheOptions = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
			};
			await _cache.SetStringAsync(cacheKey, token, cacheOptions);
		}
		//public void SaveJwtAsync(string token, int expirationMinutes)
		//{
		//	TimeSpan expiration = TimeSpan.FromMinutes(expirationMinutes);
		//	bool isSet =  _db.StringSet(token, "valid", expiration);
		//	if (!isSet) throw new Exception("Failed to save the token to Redis.");
		//}
		public async Task<string> GetTokenFromRedisAsync(int userId)
		{
			var cacheKey = $"jwt:{userId}";
			return await _cache.GetStringAsync(cacheKey);
		}

	}
}
