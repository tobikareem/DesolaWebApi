using DesolaDomain.Interface;
using DesolaDomain.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DesolaMemoryCache;

public class CacheService(IMemoryCache cache, ILogger<CacheService> logger) : ICacheService
{
    public void Add<T>(CacheEntry key, T item, int duration)
    {
        cache.Set(key, item, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(duration)
        });
    }

    public T? GetItem<T>(CacheEntry key)
    {
        return cache.Get<T>(key);
    }

    private T? Get<T>(CacheEntry key)
    {
        return cache.Get<T>(key);
    }

    public bool Contains(CacheEntry key)
    {
        return cache.TryGetValue(key, out _);
    }

    public void Remove(CacheEntry key)
    {
        cache.Remove(key);
    }

    public T? GetOrCreate<T>(CacheEntry key, Func<T> createItem, int duration)
    {
        if (cache.TryGetValue(key, out T? item))
        {
            return item;
        }

        logger.LogInformation($"Cache miss for key {key}. Item will be created.");

        try
        {
            item = createItem();
            Add(key, item, duration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create item for cache.");
            throw;
        }

        return item;
    }

}