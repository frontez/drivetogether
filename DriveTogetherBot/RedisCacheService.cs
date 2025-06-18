using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;
using System;

namespace DriveTogetherBot
{
    public class RedisCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        
        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                
                if (expiry.HasValue)
                {
                    options.SetAbsoluteExpiration(expiry.Value);
                }
                
                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in Redis cache for key {Key}", key);
                throw;
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var json = await _cache.GetStringAsync(key);
                
                if (json == null)
                {
                    return default;
                }
                
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis cache for key {Key}", key);
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from Redis cache for key {Key}", key);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence in Redis cache for key {Key}", key);
                throw;
            }
        }
    }
}