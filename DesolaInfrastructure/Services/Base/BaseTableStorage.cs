using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.Base;

public abstract class BaseTableStorage<TEntity>: ITableBase<TEntity> where TEntity : class
{

    private readonly ITableStorageRepository<TEntity> _storageRepository;
    protected readonly string TableName;
    protected readonly ICacheService CacheService;
    protected readonly ILogger Logger;
    private bool _tableInitialized;
    private readonly TimeSpan _cacheExpiration;

    protected BaseTableStorage(ITableStorageRepository<TEntity> storageRepository, ICacheService cacheService, ILogger logger, string tableName, TimeSpan? cacheExpiration = null)
    {
        _storageRepository = storageRepository;
        CacheService = cacheService;
        Logger = logger;
        TableName = tableName;
        _cacheExpiration = cacheExpiration ?? TimeSpan.FromMinutes(30);
    }

    protected virtual async Task EnsureTableExistsAsync()
    {
        if (!_tableInitialized)
        {
            await _storageRepository.CreateTableAsync(TableName);
            _tableInitialized = true;
        }
    }

    public virtual async Task InsertTableEntityAsync(TEntity entity)
    {
        await EnsureTableExistsAsync();
        await _storageRepository.InsertTableEntityAsync(TableName, entity);
        
        var cacheKey = GetCacheKey(GetPartitionKey(entity), GetRowKey(entity));
        CacheService.Add(cacheKey, entity, _cacheExpiration);
    }

    public virtual async Task<TEntity> GetTableEntityAsync(string partitionKey, string rowKey)
    {
        await EnsureTableExistsAsync();
        var cacheKey = GetCacheKey(partitionKey, rowKey);
        
        var cachedEntity = CacheService.GetItem<TEntity>(cacheKey);
        if (cachedEntity != null)
        {
            Logger.LogInformation($"Cache hit for PartitionKey: {partitionKey}, RowKey: {rowKey}");
            return cachedEntity;
        }

        try
        {
            var entity = await _storageRepository.GetTableEntityAsync(TableName, partitionKey, rowKey);
            if (entity != null)
            {
                CacheService.Add(cacheKey, entity, _cacheExpiration);
            }

            return entity;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogWarning($"Entity not found: PartitionKey '{partitionKey}', RowKey '{rowKey}'");
            return default(TEntity);
        }
    }

    public virtual async Task UpdateTableEntityAsync(TEntity entity)
    {
        await EnsureTableExistsAsync();
        var existing = await GetTableEntityAsync(GetPartitionKey(entity), GetRowKey(entity));
        if (existing == null)
        {
            throw new Exception($"Entity with PartitionKey '{GetPartitionKey(entity)}' and RowKey '{GetRowKey(entity)}' not found.");
        }

        SetETag(entity, GetETag(existing));
        await _storageRepository.UpdateTableEntityAsync(TableName, entity);
        CacheService.Remove(GetCacheKey(GetPartitionKey(entity), GetRowKey(entity)));
    }

    protected virtual string GetCacheKey(string partitionKey, string rowKey) => $"{partitionKey}:{rowKey}";
    protected abstract string GetPartitionKey(TEntity entity);
    protected abstract string GetRowKey(TEntity entity);
    protected abstract ETag GetETag(TEntity entity);
    protected abstract void SetETag(TEntity entity, ETag etag);

}