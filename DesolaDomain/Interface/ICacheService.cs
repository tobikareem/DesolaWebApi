using DesolaDomain.Model;

namespace DesolaDomain.Interface;

public interface ICacheService
{
    void Add<T>(CacheEntry key, T item, int duration);
    T? GetItem<T>(CacheEntry key);

    bool Contains(CacheEntry key);
    void Remove(CacheEntry key);
    T? GetOrCreate<T>(CacheEntry key, Func<T> createItem, int duration);
}