using System;
using System.Collections.Concurrent;
using DriveTogetherBot.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace DriveTogetherBot;

public static class Common
{
    private static RedisCacheService _redisService;
    private static ILogger<RedisCacheService> _logger;

    public static ConcurrentDictionary<long, string> Locations = new ConcurrentDictionary<long, string>();

    public static void Initialize(RedisCacheService redisService, ILogger<RedisCacheService> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    public static async Task<User> GetUser(long userId)
    {
        try
        {
            var key = $"user_{userId}";
            return await _redisService.GetAsync<User>(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId} from Redis", userId);
            throw;
        }
    }

    public static async Task AddOrUpdateUser(User user)
    {
        try
        {
            var key = $"user_{user.Id}";
            await _redisService.SetAsync(key, user, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating user {UserId} in Redis", user.Id);
            throw;
        }
    }

    public static async Task RemoveUser(long userId)
    {
        try
        {
            var key = $"user_{userId}";
            await _redisService.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from Redis", userId);
            throw;
        }
    }
}