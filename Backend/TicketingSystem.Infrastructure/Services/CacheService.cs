using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly IConnectionMultiplexer _redis;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _logger = logger;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, ct);
            if (string.IsNullOrEmpty(cachedData))
                return default;

            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data from cache for key {Key}", key);
            return default; // Fallback to DB
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting data to cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing data from cache for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                foreach (var key in keys)
                {
                    await _cache.RemoveAsync(key!, ct);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing data from cache for prefix {Prefix}", prefix);
        }
    }
}
