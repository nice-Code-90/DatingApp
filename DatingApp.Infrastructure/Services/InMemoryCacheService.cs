using System.Collections;
using System.Reflection;
using DatingApp.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DatingApp.Infrastructure.Services;

public class InMemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(memoryCache.Get<T>(key));
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
        var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
        var collection = field?.GetValue(memoryCache) as ICollection;
        return collection?.Cast<object>().Select(item => item.GetType().GetProperty("Key")?.GetValue(item)?.ToString()!).ToList() ?? new List<string>();
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        var options = new MemoryCacheEntryOptions();
        
        options.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5);

        memoryCache.Set(key, value, options);
        
        return Task.CompletedTask;
    }
}