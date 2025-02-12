
using DesolaDomain.Interfaces;
using DesolaDomain.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DesolaMemoryCache;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;
    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _memoryCache = cache;
        _logger = logger;
    }
    public void Add<T>(CacheEntry key, T item, int duration)
    {
        _memoryCache.Set(key, item, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(duration)
        });
    }

    public T? GetItem<T>(CacheEntry key)
    {
        return _memoryCache.Get<T>(key);
    }

    private T? Get<T>(CacheEntry key)
    {
        return _memoryCache.Get<T>(key);
    }

    public bool Contains(CacheEntry key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    public void Remove(CacheEntry key)
    {
        _memoryCache.Remove(key);
    }

    public T? GetOrCreate<T>(CacheEntry key, Func<T> createItem, int duration)
    {
        if (_memoryCache.TryGetValue(key, out T? item))
        {
            return item;
        }

        _logger.LogInformation($"Cache miss for key {key}. Item will be created.");

        try
        {
            item = createItem();
            Add(key, item, duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create item for _memoryCache.");
            throw;
        }

        return item;
    }

}