using DatingApp.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DatingApp.Infrastructure.Services;

public class InMemoryCacheService(IMemoryCache memoryCache) : ICacheService
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
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        var keys = GetKeys();
        var keysToRemove = keys.Where(k => k.StartsWith(prefix));

        foreach (var key in keysToRemove)
        {
            memoryCache.Remove(key);
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