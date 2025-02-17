using DesolaDomain.Enums;

namespace DesolaDomain.Interfaces;

public interface ICacheService
{
    void Add<T>(string key, T item, TimeSpan duration);
    T GetItem<T>(string key);

    bool Contains(string key);
    void Remove(string key);
    T GetOrCreate<T>(string key, Func<T> createItem, TimeSpan duration);
}