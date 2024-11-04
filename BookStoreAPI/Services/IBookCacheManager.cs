using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

public interface IBookCacheManager
{
    void AddCacheKey(string key);
    void ClearCacheKeys(IMemoryCache cache);
}

public class BookCacheManager : IBookCacheManager
{
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

    public void AddCacheKey(string key)
    {
        _cacheKeys.TryAdd(key, true);
    }

    public void ClearCacheKeys(IMemoryCache cache)
    {
        foreach (var key in _cacheKeys.Keys.ToList())
        {
            cache.Remove(key);
        }
        _cacheKeys.Clear();
    }
}
