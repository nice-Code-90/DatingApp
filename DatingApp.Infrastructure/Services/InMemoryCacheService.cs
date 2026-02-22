using DatingApp.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging; // Szükséges a logoláshoz

namespace DatingApp.Infrastructure.Services;

// Injektáljuk az ILogger-t is a konstruktorban
public class InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger) : ICacheService
{
    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(memoryCache.Get<T>(key));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5)
        };

        memoryCache.Set(key, value, options);

        // LOG: Látjuk, mi kerül be a cache-be
        logger.LogInformation("CACHE SET: Key '{Key}' added.", key);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        memoryCache.Remove(key);
        logger.LogInformation("CACHE REMOVE: Key '{Key}' removed directly.", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        var keys = GetKeys();

        // LOG: Látjuk az összes jelenlegi kulcsot és a keresett prefixet
        logger.LogInformation("CACHE PREFIX REMOVE: Searching for prefix '{Prefix}'. Current total keys in cache: {Count}", prefix, keys.Count);

        var keysToRemove = keys.Where(k => k.StartsWith(prefix)).ToList();

        if (keysToRemove.Count == 0)
        {
            logger.LogWarning("CACHE PREFIX REMOVE: No keys found starting with '{Prefix}'. Check if the prefix is correct!", prefix);
        }
        else
        {
            foreach (var key in keysToRemove)
            {
                memoryCache.Remove(key);
                logger.LogInformation("CACHE REMOVED by prefix: {Key}", key);
            }
        }

        return Task.CompletedTask;
    }

    private List<string> GetKeys()
    {
        if (memoryCache is MemoryCache concreteCache)
        {
            return concreteCache.Keys
                .Select(k => k.ToString()!)
                .ToList();
        }

        return new List<string>();
    }
}