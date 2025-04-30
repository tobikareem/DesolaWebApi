using DesolaDomain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DesolaInfrastructure.Services.Implementations;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;
    private readonly object _lock = new object();
    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _memoryCache = cache;
        _logger = logger;
    }
    public void Add<T>(string key, T item, TimeSpan duration)
    {
        lock (_lock)
        {
            _memoryCache.Set(key, item, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            });
        }
    }

    public T GetItem<T>(string key)
    {
        return _memoryCache.Get<T>(key);
    }

    public bool Contains(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public T GetOrCreate<T>(string key, Func<T> createItem, TimeSpan duration)
    {
        if (_memoryCache.TryGetValue(key, out T item))
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